using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  internal class EntitiesProcessResultBuilder {

    private readonly List<EntityInfo> newEntities;
    private readonly List<EntityInfo> existEntities;

    public EntitiesProcessResultBuilder() {
      newEntities = new List<EntityInfo>();
      existEntities = new List<EntityInfo>();
    }

    internal void AddNew(object entity, IdRev idrev, JToken doc, CouchDBViewRowKey key) {
      newEntities.Add(new EntityInfo(entity, idrev, doc, key));
    }

    internal void AddExist(object entity, IdRev idrev, JToken doc, CouchDBViewRowKey key) {
      existEntities.Add(new EntityInfo(entity, idrev, doc, key));
    }

    internal EntitiesProcessResult BuildResult() {
      return new EntitiesProcessResult(newEntities, existEntities);
    }
  }
}
