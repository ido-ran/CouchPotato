using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CouchPotato.Odm.Internal {
  /// <summary>
  /// Reperesent an embedded property definition.
  /// </summary>
  class EmbeddedPropertyDefinition : EntityPropertyDefinitionBase {

    public EmbeddedPropertyDefinition(PropertyInfo propInfo)
      : base(propInfo, StringUtil.ToCamelCase(propInfo.Name)) {
    }

    public override void Read(object entity,
      JToken doc, string id, PreProcessInfo preProcess,
      OdmViewProcessingOptions processingOptions, bool emptyProxy, CouchDBContextImpl context) {

      // Not reloading value if this is not empty proxy.
      if (emptyProxy) {
        ReadEmbeddedValue(entity, doc);
      }
    }

    public override void Write(object entity, JObject doc) {
      WriteEmbeddedValue(entity, doc);
    }

    private void WriteEmbeddedValue(object entity, JObject doc) {
      object value = PropertyInfo.GetValue(entity);
      if (!Serializer.IsNull(value)) {
        string serializedObject = JsonConvert.SerializeObject(value, Settings);
        doc.Add(JsonFieldName, JToken.Parse(serializedObject));
      }
    }

    private void ReadEmbeddedValue(object proxy, JToken doc) {
      try {
        JToken jToken = doc[JsonFieldName];

        if (jToken != null) {
          object convertedValue = JsonConvert.DeserializeObject(jToken.ToString(), PropertyInfo.PropertyType, Settings);
          if (convertedValue != null) {
            PropertyInfo.SetValue(proxy, convertedValue);
          }
        }
      }
      catch (NotImplementedException ex) {
        throw new InvalidOperationException("Fail to ready rembedded value for property " + PropertyInfo, ex);
      }
    }

    private JsonSerializerSettings Settings {
      get {
        return new JsonSerializerSettings
        {
          ContractResolver = new CamelCasePropertyNamesContractResolver(),
          NullValueHandling = NullValueHandling.Ignore
        };
      }
    }

  }
}
