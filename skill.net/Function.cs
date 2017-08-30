using Amazon.Lambda.Core;

using Alexa.NET.Request;
using Alexa.NET.Response;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace skill.net
{
  public class Function
  {
    public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
    {
      context.Logger.LogLine("Got input");

      return new RequestHandler<AudioSessionAttributes>(
        input,
        context,
        new PodcastAudioStateEventHandler(context.Logger)).Execute();
    }
  }
}
