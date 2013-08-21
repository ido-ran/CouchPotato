
namespace CouchPotato.Odm {
  /// <summary>
  /// Represent an assoication collection.
  /// </summary>
  internal interface AssociationCollection {
    /// <summary>
    /// Get the number of elements in the collection without materializing it.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Get the entities Ids without materializing the collection.
    /// </summary>
    /// <returns></returns>
    string[] GetEntityIds();
  }
}
