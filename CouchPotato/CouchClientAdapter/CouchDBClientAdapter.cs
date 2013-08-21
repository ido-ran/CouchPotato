using Newtonsoft.Json.Linq;

namespace CouchPotato.CouchClientAdapter {
  /// <summary>
  /// Represent CouchDB client.
  /// </summary>
  public interface CouchDBClientAdapter {
    /// <summary>
    /// Query a view for rows.
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="viewOptions"></param>
    /// <returns></returns>
    JToken[] GetViewRows(string viewName, CouchViewOptions viewOptions);

    /// <summary>
    /// Create bulk updater.
    /// </summary>
    /// <returns></returns>
    BulkUpdater CreateBulkUpdater();
  }
}
