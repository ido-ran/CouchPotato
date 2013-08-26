using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm {
  internal class OdmViewProcessingOptions {
    private Dictionary<string, object> assoicateCollectionsToLoad;

    public OdmViewProcessingOptions() {
      assoicateCollectionsToLoad = new Dictionary<string, object>();
    }

    public OdmViewProcessingOptions(IEnumerable<Tuple<string, object>> assoicateCollectionsToLoad) {
      this.assoicateCollectionsToLoad = assoicateCollectionsToLoad.ToDictionary(x => x.Item1, x => x.Item2);
    }

    public Dictionary<string, object> AssoicateCollectionsToLoad {
      get { return assoicateCollectionsToLoad; }
    }
  }
}
