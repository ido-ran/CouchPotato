using CouchPotato.Odm;

namespace CouchPotato.Migration {
  /// <summary>
  /// Provide common base class for MigrationDefinition.
  /// </summary>
  public abstract class MigrationDefinitionBase : MigrationDefinition {

    private readonly string name;

    public MigrationDefinitionBase() {
      // Use the class name as the migration name.
      this.name = GetType().Name;
    }

    public string Name {
      get { return name; }
    }

    public abstract void Up(CouchDBContextImpl context);
  }
}
