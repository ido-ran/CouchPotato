using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm {
  internal class LoadRelatedPreProcessInfoBuilder {
    private readonly HashSet<string> relatedEntityIdsToLoad;
    private readonly Dictionary<string, List<string>> viewsToSelect;

    public LoadRelatedPreProcessInfoBuilder() {
      relatedEntityIdsToLoad = new HashSet<string>();
      viewsToSelect = new Dictionary<string, List<string>>();
    }

    public ICollection<string> RelatedEntityIdsToLoad {
      get { return relatedEntityIdsToLoad; }
    }

    public void AddViewSelection(string viewName, string entityId) {
      List<string> entityIds;

      if (!viewsToSelect.TryGetValue(viewName, out entityIds)) {
        entityIds = new List<string>();
        viewsToSelect.Add(viewName, entityIds);
      }

      entityIds.Add(entityId);
    }

    public LoadRelatedPreProcessInfo Build() {
      return new LoadRelatedPreProcessInfo(
        relatedEntityIdsToLoad.ToArray(),
        viewsToSelect.ToDictionary(x => x.Key, x => x.Value.ToArray()));
    }

  }
}
