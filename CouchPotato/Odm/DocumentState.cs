
namespace CouchPotato.Odm {
  /// <summary>
  /// Represent the states in which a document can be in the context.
  /// </summary>
  internal enum DocumentState {
    /// <summary>
    /// Document is clean, same as in the database.
    /// </summary>
    Clean,

    /// <summary>
    /// Document was modified locally.
    /// </summary>
    Modified,

    /// <summary>
    /// New entity
    /// </summary>
    New
  }
}
