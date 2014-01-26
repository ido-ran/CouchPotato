using System.Collections.Generic;

namespace CouchPotato.Migration {
  /// <summary>
  /// Represent a list of migrations that require for the application.
  /// </summary>
  public class RequiredMigrations : List<MigrationDefinition> {
  }
}
