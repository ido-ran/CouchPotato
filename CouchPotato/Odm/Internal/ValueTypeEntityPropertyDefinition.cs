using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm.Internal {
  internal class ValueTypeEntityPropertyDefinition : EntityPropertyDefinitionBase {

    public ValueTypeEntityPropertyDefinition(PropertyInfo propInfo)
      : base(propInfo, StringUtil.ToCamelCase(propInfo.Name)) {
    }

    protected ValueTypeEntityPropertyDefinition(PropertyInfo propInfo, string jsonFieldName)
      : base(propInfo, jsonFieldName) {

    }

    public override void Read(object entity, JToken doc, string id,
      PreProcessInfo preProcess, OdmViewProcessingOptions processingOptions, bool emptyProxy, CouchDBContextImpl context) {

      // Not reloading value if this is not empty proxy.
      if (emptyProxy) {
        ReadValueType(entity, doc);
      }
    }

    public override void Write(object entity, JObject doc) {
      WriteValueType(entity, doc);
    }

    private void WriteValueType(object entity, JObject doc) {
      if (doc[JsonFieldName] == null) {
        object value = PropertyInfo.GetValue(entity);
        if (!Serializer.IsNull(value)) {
          doc.Add(JsonFieldName, new JValue(value));
        }
      }
    }

    private void ReadValueType(object proxy, JToken doc) {
      try {
        JToken jToken = doc[JsonFieldName];

        object convertedValue = Serializer.ResolveValue(jToken, PropertyInfo.PropertyType);
        if (convertedValue != null) {
          PropertyInfo.SetValue(proxy, convertedValue);
        }
      }
      catch (NotImplementedException ex) {
        throw new InvalidOperationException("Fail to ready value for property " + PropertyInfo, ex);
      }
    }
  }
}
