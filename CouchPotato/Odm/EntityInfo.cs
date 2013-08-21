namespace CouchPotato.Odm {
  /// <summary>
  /// Represent entity instance with IdRev pair.
  /// </summary>
  internal class EntityInfo {
    private readonly object entity;
    private readonly IdRev idrev;

    public EntityInfo(object entity, IdRev idrev) {
      this.entity = entity;
      this.idrev = idrev;
    }

    public object Entity {
      get { return entity; }
    }

    public IdRev IdRev {
      get { return idrev; }
    }
  }
}