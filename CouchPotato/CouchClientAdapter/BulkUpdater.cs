using Newtonsoft.Json.Linq;
namespace CouchPotato.CouchClientAdapter {
  /// <summary>
  /// Represent implementation for CouchDB builk update API.
  /// </summary>
  public interface BulkUpdater {

    /// <summary>
    /// Add document to be updated in bulk.
    /// </summary>
    /// <param name="entityAsDoc"></param>
    void Update(JObject entityAsDoc);

    /// <summary>
    /// Execute the bulk operation.
    /// </summary>
    /// <returns></returns>
    BulkResponse Execute();
  }
}
