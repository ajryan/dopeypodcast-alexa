using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;

namespace skill.net
{
  public interface IStateEventHandler<in TAttributes> where TAttributes : SessionAttributes
  {
    SkillResponse HandleStateEvent(string state, string @event, TAttributes attributes, ILambdaContext context, Request request);
  }

  public class RequestHandler<TAttributes> where TAttributes : SessionAttributes, new()
  {
    private readonly IStateEventHandler<TAttributes> _stateEventHandler;
    public SkillRequest Event { get; }
    public ILambdaContext Context { get; }
    public string UserId { get; }
    public TAttributes Attributes { get; private set; }

    public RequestHandler(SkillRequest @event, ILambdaContext context, IStateEventHandler<TAttributes> stateEventHandler)
    {
      Event = @event;
      Context = context;
      UserId = @event.Context?.System.User.UserId ?? @event.Session.User.UserId;

      _stateEventHandler = stateEventHandler;
    }

    public SkillResponse Execute()
    {
      Attributes = LoadUserAttributes();

      string eventString =
        Event.Request is LaunchRequest                   ? Constants.Events.LaunchRequest
          : Event.Request is IntentRequest intentRequest ? intentRequest.Intent.Name
          : Event.Request is SessionEndedRequest         ? Constants.Events.SessionEndedRequest
                                                         : Constants.Events.Unhandled;

      Context.Logger.LogLine($"Invoking handler in state = {Attributes.State}; event {eventString}; userId {UserId}");

      var response = _stateEventHandler.HandleStateEvent(
        Attributes.State,
        eventString,
        Attributes,
        Context,
        Event.Request);

      response.SessionAttributes = Attributes.GetAlexaValueMap();

      SaveUserAttributes();

      return response;
    }

    private TAttributes LoadUserAttributes()
    {
      var dynamoClient = new AmazonDynamoDBClient();
      var table = Table.LoadTable(dynamoClient, Constants.DynamoTableName);

      var documentResponse = table.GetItemAsync(new Primitive(UserId)).Result;
      var attributes = new TAttributes();

      if (documentResponse == null)
        attributes.InitializeNew(UserId, Constants.States.StartMode);
      else
        attributes.Initialize(documentResponse.ToAttributeMap());

      return attributes;
    }

    private void SaveUserAttributes()
    {
      var dynamoClient = new AmazonDynamoDBClient();
      var table = Table.LoadTable(dynamoClient, Constants.DynamoTableName);
      var documentResponse = table.PutItemAsync(Document.FromAttributeMap(Attributes.DynamoValueMap)).Result;
    }
  }
}