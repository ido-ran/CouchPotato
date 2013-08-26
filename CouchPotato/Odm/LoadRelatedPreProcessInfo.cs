using System.Collections.Generic;

namespace CouchPotato.Odm {
  /// <summary>
  /// LoadRelated pre process information.
  /// </summary>
  internal class LoadRelatedPreProcessInfo {
    private readonly string[] entityIdsToLoad;
    private readonly Dictionary<string, string[]> viewsToSelect;

    public LoadRelatedPreProcessInfo(string[] entityIdsToLoad, Dictionary<string, string[]> viewsToSelect) {
      this.entityIdsToLoad = entityIdsToLoad;
      this.viewsToSelect = viewsToSelect;
    }

    /// <summary>
    /// Get all the entity IDs to load.
    /// </summary>
    public string[] EntityIdsToLoad {
      get { return entityIdsToLoad; }
    }

    /// <summary>
    /// Get views to select with list of keys to select from them.
    /// </summary>
    public Dictionary<string, string[]> ViewsToSelect {
      get { return viewsToSelect; }
    }
  }
}
