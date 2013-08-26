using Newtonsoft.Json.Linq;
namespace CouchPotato.Odm {
  /// <summary>
  /// Represent entity instance with IdRev pair.
  /// </summary>
  internal class EntityInfo {
    private readonly object entity;
    private readonly IdRev idrev;
    private readonly JToken doc;
    private readonly CouchDBViewRowKey key;

    public EntityInfo(object entity, IdRev idrev, JToken doc, CouchDBViewRowKey key) {
      this.entity = entity;
      this.idrev = idrev;
      this.doc = doc;
      this.key = key;
    }

    public object Entity {
      get { return entity; }
    }

    public IdRev IdRev {
      get { return idrev; }
    }

    public JToken Document {
      get { return doc; }
    }

    public CouchDBViewRowKey Key {
      get { return key; }
    }
  }
}