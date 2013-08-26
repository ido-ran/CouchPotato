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
    /// Select documents by ids.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    JToken[] GetDocuments(string[] ids);

    /// <summary>
    /// Create bulk updater.
    /// </summary>
    /// <returns></returns>
    BulkUpdater CreateBulkUpdater();
  }
}
