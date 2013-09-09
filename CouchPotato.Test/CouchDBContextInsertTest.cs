using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CouchPotato.Test {
  [TestClass]
  public class CouchDBContextInsertTest {
    [TestMethod]
    public void InsertTenantEntity_ValidateModifiedDocumentsCount() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      var couchDBClientMock = new CouchDBClientAdapterMock(null, bulkUpdaterMock);
      var subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);

      TenantModel newTenant = new TenantModel
      {
        TenantModelID = "2001",
        Name = "Tin-Tenant"
      };

      subject.Update(newTenant);

      CouchPotatoAssert.ModifiedDocumentsCount(subject, 1);
    }

    [TestMethod]
    public void InsertTenantEntity_ValidateSaveChanges() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      var couchDBClientMock = new CouchDBClientAdapterMock(null, bulkUpdaterMock);
      var subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);

      TenantModel newTenant = new TenantModel
      {
        TenantModelID = "2001",
        Name = "Tin-Tenant"
      };

      subject.Update(newTenant);
      subject.SaveChanges();

      Assert.AreEqual(1, bulkUpdaterMock.EntitiesToUpdate.Count);
      Assert.AreEqual("2001", bulkUpdaterMock.EntitiesToUpdate[0].Value<string>("_id"));
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void InsertEntityThatIsNotMapped() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      var couchDBClientMock = new CouchDBClientAdapterMock(null, bulkUpdaterMock);
      var subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);

      var entityInstance = new UnmappedEntity { UnmappedEntityID = "A" };
      subject.Update(entityInstance);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void InsertEntityThatIsNotMappedWithoutIDField() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      var couchDBClientMock = new CouchDBClientAdapterMock(null, bulkUpdaterMock);
      var subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);

      var entityInstance = new UnmappedWithoutIDEntity();
      subject.Update(entityInstance);
    }

    private class UnmappedEntity {
      public string UnmappedEntityID { get; set; }
    }

    private class UnmappedWithoutIDEntity {
    }
  }
}
