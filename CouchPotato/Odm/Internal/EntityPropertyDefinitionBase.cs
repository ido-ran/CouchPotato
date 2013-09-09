using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm.Internal {
  internal abstract class EntityPropertyDefinitionBase : EntityPropertyDefinition {

    private readonly PropertyInfo propInfo;
    private readonly string jsonFieldName;

    public EntityPropertyDefinitionBase(PropertyInfo propInfo, string jsonFieldName) {
      this.propInfo = propInfo;
      this.jsonFieldName = jsonFieldName;
    }

    public abstract void Read(object entity, JToken doc, string id, 
      PreProcessInfo preProcess, OdmViewProcessingOptions processingOptions, bool emptyProxy,
      CouchDBContext context);

    public abstract void Write(object entity, Newtonsoft.Json.Linq.JObject doc);

    public string JsonFieldName { get { return jsonFieldName; } }

    protected PropertyInfo PropertyInfo { get { return propInfo; } }

    public override string ToString() {
      return string.Format("{0} {1} {2}", GetType().Name, jsonFieldName, propInfo.Name);
    }
  }
}
