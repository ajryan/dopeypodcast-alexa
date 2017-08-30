using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Amazon.DynamoDBv2.Model;

namespace skill.net
{
  public abstract class SessionAttributes
  {
    public string UserId { get => Get<string>(); set => Set(value); }
    public string State { get => Get<string>(); set => Set(value); }

    public Dictionary<string, AttributeValue> DynamoValueMap { get; private set; }

    protected SessionAttributes()
    {
      DynamoValueMap = new Dictionary<string, AttributeValue>();
    }

    public virtual void InitializeNew(string userId, string initialState)
    {
      UserId = userId;
      State = initialState;
    }

    public void Initialize(Dictionary<string, AttributeValue> dynamoValueMap)
    {
      DynamoValueMap = dynamoValueMap;
    }

    public Dictionary<string, object> GetAlexaValueMap()
    {
      return GetType().GetProperties()
        .Where(aProperty => aProperty.Name != "DynamoValueMap")
        .ToDictionary(
            property => property.Name,
            property => property.GetValue(this));
    }

    protected T Get<T>([CallerMemberName] string key = null)
    {
      key = NormalizeKey(key);

      if (!DynamoValueMap.ContainsKey(key))
        return default(T);

      var attributeValue = DynamoValueMap[key];

      if (typeof(T) == typeof(string))
        return (T)(object)attributeValue.S;

      if (typeof(T) == typeof(int))
        return (T)(object)Int32.Parse(attributeValue.N);

      if (typeof(T) == typeof(long))
        return (T)(object)Int64.Parse(attributeValue.N);

      if (typeof(T) == typeof(bool))
        return (T)(object)attributeValue.BOOL;

      if (typeof(T) == typeof(List<int>))
        return (T)(object)attributeValue.NS.Select(Int32.Parse).ToList();

      throw new InvalidOperationException($"Cannot get vlue of type {typeof(T)}.");
    }

    protected void Set(object value, [CallerMemberName] string key = null)
    {
      key = NormalizeKey(key);

      var attribute =
          value is string stringValue  ? new AttributeValue { S = stringValue }
        : value is int intValue        ? new AttributeValue { N = intValue.ToString() }
        : value is long longValue      ? new AttributeValue { N = longValue.ToString() }
        : value is bool boolValue      ? new AttributeValue { BOOL = boolValue }
        : value is List<int> listValue ? new AttributeValue { NS = listValue.Select(n => n.ToString()).ToList() }
                                       : throw new InvalidOperationException($"Cannot store value of type {value.GetType()}.");

      if (DynamoValueMap.ContainsKey(key))
        DynamoValueMap[key] = attribute;
      else
        DynamoValueMap.Add(key, attribute);
    }

    private static string NormalizeKey(string key)
    {
      if (key == null)
        throw new ArgumentNullException(nameof(key));

      key = key.Substring(0, 1).ToLower() + key.Substring(1);
      return key;
    }
  }
  public class AudioSessionAttributes : SessionAttributes
  {
    public List<int> PlayOrder { get => Get<List<int>>(); set => Set(value); }
    public int VisitCount { get => Get<int>(); set => Set(value); }
    public int Index { get => Get<int>(); set => Set(value); }
    public long OffsetInMilliseconds { get => Get<long>(); set => Set(value); }
    public bool Loop { get => Get<bool>(); set => Set(value); }
    public bool Shuffle { get => Get<bool>(); set => Set(value); }
    public bool PlaybackIndexChanged { get => Get<bool>(); set => Set(value); }
    public bool PlaybackFinished { get => Get<bool>(); set => Set(value); }
    public int EnqueuedToken { get => Get<int>(); set => Set(value); }

    public override void InitializeNew(string userId, string initialState)
    {
      base.InitializeNew(userId, initialState);

      VisitCount = 0;
    }
  }
}
