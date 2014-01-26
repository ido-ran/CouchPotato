using System;
using CouchPotato.CouchClientAdapter;
using CouchPotato.Migration;
using CouchPotato.Odm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CouchPotato.Test {
  [TestClass]
  public class MigratorTest {
    [TestMethod]
    public void NoMigrationToExecute() {
      var requiredMigrations = new RequiredMigrations();
      requiredMigrations.Add(new MigrationMock("001Migration", MigrationExcpectation.NotToRun));

      var existMigrations = new ExistMigrations();
      existMigrations.Add(new ExistMigrationInfo("001Migration"));

      var context = new CouchDBContextImpl(null);
      var subject = new MigratorUnderTest(context, requiredMigrations, existMigrations);

      subject.Migrate();

      EnsureExpectedMigrationRan(requiredMigrations);
    }

    [TestMethod]
    public void OneMigrationToExecute() {
      var requiredMigrations = new RequiredMigrations();
      requiredMigrations.Add(new MigrationMock("001Migration", MigrationExcpectation.NotToRun));
      requiredMigrations.Add(new MigrationMock("002Migration", MigrationExcpectation.Run));

      var existMigrations = new ExistMigrations();
      existMigrations.Add(new ExistMigrationInfo("001Migration"));

      var context = new CouchDBContextImpl(null);
      var subject = new MigratorUnderTest(context, requiredMigrations, existMigrations);

      subject.Migrate();

      EnsureExpectedMigrationRan(requiredMigrations);
    }

    [TestMethod]
    public void FiveMigrationToExecute() {
      var requiredMigrations = new RequiredMigrations();
      requiredMigrations.Add(new MigrationMock("001Migration", MigrationExcpectation.NotToRun));
      requiredMigrations.Add(new MigrationMock("002Migration", MigrationExcpectation.Run));
      requiredMigrations.Add(new MigrationMock("003Migration", MigrationExcpectation.Run));
      requiredMigrations.Add(new MigrationMock("004Migration", MigrationExcpectation.Run));
      requiredMigrations.Add(new MigrationMock("005Migration", MigrationExcpectation.Run));
      requiredMigrations.Add(new MigrationMock("006Migration", MigrationExcpectation.Run));

      var existMigrations = new ExistMigrations();
      existMigrations.Add(new ExistMigrationInfo("001Migration"));

      var context = new CouchDBContextImpl(null);
      var subject = new MigratorUnderTest(context, requiredMigrations, existMigrations);

      subject.Migrate();

      EnsureExpectedMigrationRan(requiredMigrations);
    }

    [TestMethod]
    [ExpectedException(typeof(MigrationDivergedException))]
    public void DatabaseIsMoreUpdatedThanCode() {
      var requiredMigrations = new RequiredMigrations();
      requiredMigrations.Add(new MigrationMock("001Migration", MigrationExcpectation.NotToRun));
      requiredMigrations.Add(new MigrationMock("002Migration", MigrationExcpectation.NotToRun));

      var existMigrations = new ExistMigrations();
      existMigrations.Add(new ExistMigrationInfo("001Migration"));
      existMigrations.Add(new ExistMigrationInfo("002Migration"));
      existMigrations.Add(new ExistMigrationInfo("003Migration"));

      var context = new CouchDBContextImpl(null);
      var subject = new MigratorUnderTest(context, requiredMigrations, existMigrations);

      subject.Migrate();

      EnsureExpectedMigrationRan(requiredMigrations);
    }

    private void EnsureExpectedMigrationRan(RequiredMigrations requiredMigrations) {
      foreach (MigrationMock migrationMock in requiredMigrations) {
        migrationMock.EnsureExpectedResult();
      }
    }

    private class MigratorUnderTest : Migrator {

      private readonly ExistMigrations existMigrations;

      public MigratorUnderTest(
        CouchDBContextImpl context, 
        RequiredMigrations requiredMigrations, 
        ExistMigrations existMigrations)
      : base(context, requiredMigrations) {

        this.existMigrations = existMigrations;
      }

      protected override ExistMigrations LoadExistMigrations() {
        return existMigrations;
      }

      protected override void WriteMigrationApplied(MigrationDefinition migrationDef) {
        // We ignore this method for the tests.
      }
    }

    private enum MigrationExcpectation {
      Run,
      NotToRun
    }

    private class MigrationMock : MigrationDefinition {

      private readonly string name;
      private readonly MigrationExcpectation expectation;

      private int runCount;

      public MigrationMock(string name, MigrationExcpectation expectation) {
        this.name = name;
        this.expectation = expectation;
      }

      public void Up(CouchDBContextImpl context) {
        if (expectation == MigrationExcpectation.NotToRun) {
          Assert.Fail("Migration " + name + " should not have run but it was");
        }
        runCount++;
      }

      public void EnsureExpectedResult() {
        if (expectation == MigrationExcpectation.Run && runCount == 0) {
          Assert.Fail("Migration " + name + " should have run but it didn't");
        }

        if (runCount > 1) {
          Assert.Fail("Migration " + name + " ran more than once. It run " + runCount + " times");
        }
      }

      public string Name {
        get { return name; }
      }
    }


  }
}
