using CouchPotato.Odm;

namespace CouchPotato.Migration {

  /// <summary>
  /// Represent a migration operation.
  /// </summary>
  public interface MigrationDefinition {

    /// <summary>
    /// Get the name of the migration.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Migrate to the next version.
    /// </summary>
    /// <param name="context">Context to access CouchDB.</param>
    void Up(CouchDBContextImpl context);
  }
}
