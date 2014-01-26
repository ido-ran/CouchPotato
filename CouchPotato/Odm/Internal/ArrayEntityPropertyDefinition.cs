using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm.Internal {
  class ArrayEntityPropertyDefinition : EntityPropertyDefinitionBase {

    public ArrayEntityPropertyDefinition(PropertyInfo propInfo)
      : base(propInfo, StringUtil.ToCamelCase(propInfo.Name)) {
    }

    public override void Read(object entity, JToken doc, string id, PreProcessInfo preProcess,
      OdmViewProcessingOptions processingOptions, bool emptyProxy, CouchDBContextImpl context) {

      ReadArray(entity, doc);
    }

    public override void Write(object entity, JObject doc) {
      WriteArray(entity, doc);
    }

    private void ReadArray(object proxy, JToken doc) {
      Array arr = Serializer.GetJsonArray(PropertyInfo, doc);
      if (arr != null) {
        PropertyInfo.SetValue(proxy, arr);
      }
    }

    private void WriteArray(object entity, JObject doc) {
      object value = PropertyInfo.GetValue(entity);
      if (!Serializer.IsNull(value)) {
        doc.Add(JsonFieldName, new JArray(value));
      }
    }
  }
}
