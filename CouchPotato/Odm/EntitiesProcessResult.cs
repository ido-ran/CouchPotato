using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm {
  /// <summary>
  /// Represent the result of processing CouchDB documents fetch.
  /// </summary>
  internal class EntitiesProcessResult {

    private readonly List<EntityInfo> newEntities;
    private readonly List<EntityInfo> existEntities;

    public EntitiesProcessResult(List<EntityInfo> newEntities, List<EntityInfo> existEntities) {
      this.newEntities = newEntities;
      this.existEntities = existEntities;
    }

    /// <summary>
    /// Get all the entities of this process.
    /// </summary>
    public List<object> Entities {
      get {
        return
            AllEntities
            .Select(x => x.Entity)
            .ToList();            
      }
    }

    private IEnumerable<EntityInfo> AllEntities {
      get {
        return newEntities.Concat(existEntities);
      }
    }

    /// <summary>
    /// Get entities found in this selection.
    /// </summary>
    public List<EntityInfo> NewEntities {
      get { return newEntities; }
    }

    internal object GetEntity(string relatedEntityId) {
      var foundEntity =
        from e in AllEntities
        where e.IdRev.Id.Equals(relatedEntityId)
        select e.Entity;

      return foundEntity.SingleOrDefault();
    }

    /// <summary>
    /// Select document ids by view's row key.
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    internal string[] GetRelatedEntitiesIds(string entityId) {
      var q =
        from e in AllEntities
        where e.Key.RawKey.Equals(entityId)
        select e.IdRev.Id;

      return q.ToArray();
    }
  }
}
