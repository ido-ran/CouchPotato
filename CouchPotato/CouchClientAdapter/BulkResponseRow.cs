
namespace CouchPotato.CouchClientAdapter {
  /// <summary>
  /// Represent the bulk response row per document.
  /// </summary>
  public class BulkResponseRow {

    private readonly string id;
    private readonly string rev;
    private readonly string error;
    private readonly string reason;

    public BulkResponseRow(
      string id,
      string rev,
      string error,
      string reason) {

        this.id = id;
        this.rev = rev;
        this.error = error;
        this.reason = reason;
    }

    public string Id { get { return id; } }
    public string Rev { get { return rev; } }
    public string Error { get { return error; } }
    public string Reason { get { return reason; } }

    /// <summary>
    /// Get indication if the result row has error.
    /// </summary>
    public bool HasError {
      get { return !string.IsNullOrEmpty(error); }
    }
  }
}
