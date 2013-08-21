namespace CouchPotato.Odm {
  /// <summary>
  /// Repersent a document _id and _rev pair.
  /// </summary>
  internal class IdRev {
    private readonly string id;
    private readonly string rev;

    public IdRev(string id, string rev) {
      this.id = id;
      this.rev = rev;
    }

    /// <summary>
    /// Get the Id.
    /// </summary>
    public string Id { get { return id; } }

    /// <summary>
    /// Get the Rev.
    /// </summary>
    public string Rev { get { return rev; } }
  }
}
