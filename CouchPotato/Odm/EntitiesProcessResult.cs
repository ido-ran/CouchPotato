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
          newEntities.Concat(existEntities)
            .Select(x => x.Entity)
            .ToList();            
      }
    }

    /// <summary>
    /// Get entities found in this selection.
    /// </summary>
    public List<EntityInfo> NewEntities {
      get { return newEntities; }
    }
  }
}
