namespace CouchPotato.Migration {
  /// <summary>
  /// Contain information about migration that was already executed.
  /// </summary>
  public class ExistMigrationInfo {
    private readonly string name;

    public ExistMigrationInfo(string name) {
      this.name = name;
    }

    public string Name { get { return name; } }
  }
}
