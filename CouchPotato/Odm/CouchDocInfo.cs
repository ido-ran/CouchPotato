using Newtonsoft.Json.Linq;
namespace CouchPotato.Odm {
  /// <summary>
  /// Represent the information stored for each document
  /// to allow updates of loaded documents.
  /// </summary>
  internal class CouchDocInfo {

    private readonly string id;
    private readonly string rev;
    private readonly DocumentState state;
    private readonly JToken document;

    public CouchDocInfo(string id, string rev, JToken document, DocumentState state) {
      this.id = id;
      this.rev = rev;
      this.document = document;
      this.state = state;
    }

    /// <summary>
    /// The document _id field.
    /// </summary>
    public string Id { get { return id; } }

    /// <summary>
    /// The last known revision of the document.
    /// </summary>
    public string Rev { get { return rev; } }

    /// <summary>
    /// The document from which the entity was loaded.
    /// </summary>
    public JToken Document { get { return document; } }

    /// <summary>
    /// The state of the document in the context.
    /// </summary>
    public DocumentState State { get { return state; } }

    internal CouchDocInfo ChangeState(DocumentState newState) {
      if (state == DocumentState.New) return this;
      else return new CouchDocInfo(id, rev, document, newState);
    }

    internal CouchDocInfo ChangeState(DocumentState newState, string newRev) {
      return new CouchDocInfo(id, newRev, document, newState);
    }

    public override string ToString() {
      return string.Format("{0} {1} {2} {3}", GetType().Name, id, state, rev);
    }
  }
}
