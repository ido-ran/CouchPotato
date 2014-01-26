using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm.Internal {
  class ToOneEntityPropertyDefinition : EntityPropertyDefinitionBase {

    public ToOneEntityPropertyDefinition(PropertyInfo prop)
      : base(prop, StringUtil.ToCamelCase(prop.Name) + "ID") {
    }

    private void WriteToOneReference(object entity, JObject doc) {
      object relatedEntity = PropertyInfo.GetValue(entity);
      if (!Serializer.IsNull(relatedEntity)) {
        string relatedEntityId = CouchDBContextImpl.GetEntityInstanceId(relatedEntity);
        doc.Add(JsonFieldName, relatedEntityId);
      }
    }

    public override void Read(object entity, JToken doc, string id, PreProcessInfo preProcess, OdmViewProcessingOptions processingOptions, bool emptyProxy, CouchDBContextImpl context) {
      
      // TODO: implement this.
    }

    public override void Write(object entity, JObject doc) {
      WriteToOneReference(entity, doc);
    }
  }
}
