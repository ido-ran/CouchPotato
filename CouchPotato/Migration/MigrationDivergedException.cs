using System;

namespace CouchPotato.Migration {
  /// <summary>
  /// Occurs when the database has applied migration that are not
  /// exist in the required migrations.
  /// </summary>
  [Serializable]
  public class MigrationDivergedException : System.Exception {
    private ExistMigrationInfo[] divergedMigrations;

    public MigrationDivergedException() { }
    public MigrationDivergedException(string message) : base(message) { }
    public MigrationDivergedException(string message, System.Exception inner) : base(message, inner) { }
    protected MigrationDivergedException(
    System.Runtime.Serialization.SerializationInfo info,
    System.Runtime.Serialization.StreamingContext context)
      : base(info, context) { }

    public MigrationDivergedException(string message, ExistMigrationInfo[] divergedMigrations) 
      : base(message) {
 
      this.divergedMigrations = divergedMigrations;
    }
  }
}
