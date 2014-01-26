using System.Collections.Generic;
using CouchPotato.Odm;
using System.Linq;
using Newtonsoft.Json.Linq;
using System;
using CouchPotato.CouchClientAdapter;

namespace CouchPotato.Migration {
  /// <summary>
  /// Perform migrations on CouchDB.
  /// </summary>
  public class Migrator {
    private readonly CouchDBContextImpl context;
    private readonly RequiredMigrations requiredMigrations;

    private JObject migrationsDoc;

    private const string AppliedMigrationField = "applied";
    private const string MigrationsDocumentId = "migrations_";
    private const string MigrationInfoNameField = "name";
    private const string MigrationInfoDateField = "date";

    public Migrator(CouchDBContextImpl context, RequiredMigrations requiredMigrations) {
      this.context = context;
      this.requiredMigrations = requiredMigrations;
    }

    /// <summary>
    /// Perform migration.
    /// </summary>
    public void Migrate() {
      MigrationDefinition[] migrationsToExecute = CalculateMigrationsToExecute();
      foreach (MigrationDefinition migrationDef in migrationsToExecute) {
        // Before we apply the migration we write it to the database.
        // It is prefered to fail with evedence of what happen than no data at all.
        WriteMigrationApplied(migrationDef);

        // Execute the migration.
        migrationDef.Up(context);
      }
    }

    protected virtual void WriteMigrationApplied(MigrationDefinition migrationDef) {
      // Get migrations array
      JArray appliedMigrationsArray = migrationsDoc.Value<JArray>(AppliedMigrationField);
      JObject migrationObj = BuildMigrationJObject(migrationDef);
      appliedMigrationsArray.Add(migrationObj);

      string newRev = UpdateMigrationsDocument();
      migrationsDoc["_rev"] = new JValue(newRev);
    }

    private string UpdateMigrationsDocument() {
      BulkUpdater bulkUpdater = context.ClientAdaper.CreateBulkUpdater(allOrNothing: false);
      bulkUpdater.Update(migrationsDoc);
      BulkResponse bulkResponse = bulkUpdater.Execute();

      string newRev = bulkResponse.Rows[0].Rev;
      return newRev;
    }

    private static JObject BuildMigrationJObject(MigrationDefinition migrationDef) {
      JObject migrationObj = new JObject();
      migrationObj.Add(MigrationInfoNameField, migrationDef.Name);
      migrationObj.Add(MigrationInfoDateField, new JValue(DateTime.UtcNow));
      return migrationObj;
    }

    protected virtual ExistMigrations LoadExistMigrations() {
      ExistMigrations existMigrations = new ExistMigrations();

      migrationsDoc = context.ClientAdaper.GetDocument(MigrationsDocumentId);
      if (migrationsDoc == null) {
        migrationsDoc = CreateEmptyMigrationsDoc();
      }

      JArray appliedArray = migrationsDoc.Value<JArray>(AppliedMigrationField);
      foreach (JToken appliedMigrationObj in appliedArray) {
        string migrationName = appliedMigrationObj.Value<string>(MigrationInfoNameField);
        existMigrations.Add(new ExistMigrationInfo(migrationName));
      }

      return existMigrations;
    }

    private JObject CreateEmptyMigrationsDoc() {
      JObject emptyMigrationsDoc = new JObject();
      emptyMigrationsDoc.Add("_id", MigrationsDocumentId);
      emptyMigrationsDoc.Add(AppliedMigrationField, new JArray());

      return emptyMigrationsDoc;
    }

    private MigrationDefinition[] CalculateMigrationsToExecute() {
      ExistMigrations existMigrations = LoadExistMigrations();

      var requiredMigrationNames = new HashSet<string>(
        requiredMigrations.Select(x=>x.Name));

      ExistMigrationInfo[] divergedMigrations =
        existMigrations
        .Where(x => !requiredMigrationNames.Contains(x.Name))
        .ToArray();

      if (divergedMigrations.Length > 0) {
        string errMsg = "The database has applied migrations that do not exist in the required migrations.";
        throw new MigrationDivergedException(errMsg, divergedMigrations);
      }

      var existMigrationNames = new HashSet<string>(
        existMigrations.Select(x => x.Name));

      MigrationDefinition[] migrationsToExecute = 
        requiredMigrations
        .Where(x => !existMigrationNames.Contains(x.Name))
        .ToArray();

      return migrationsToExecute;
    }
  }
}
