
namespace CouchPotato.CouchClientAdapter {
  /// <summary>
  /// Represent CouchDB bulk response.
  /// </summary>
  public class BulkResponse {

    private readonly BulkResponseRow[] rows;

    public BulkResponse(BulkResponseRow[] rows) {
      this.rows = rows;
    }

    public BulkResponseRow[] Rows {
      get { return rows; }
    }

    public bool IsEmpty {
      get { return Rows == null || Rows.Length == 0; }
    }
  }
}
