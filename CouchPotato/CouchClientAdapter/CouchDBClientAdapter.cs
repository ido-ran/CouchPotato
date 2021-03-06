﻿using Newtonsoft.Json.Linq;

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
    /// <param name="allOrNothing"></param>
    /// <returns></returns>
    BulkUpdater CreateBulkUpdater(bool allOrNothing);

    /// <summary>
    /// Get a single document by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    JObject GetDocument(string id);
  }
}
