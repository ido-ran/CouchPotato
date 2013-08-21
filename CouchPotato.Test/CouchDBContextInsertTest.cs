using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CouchPotato.Test {
  [TestClass]
  public class CouchDBContextInsertTest {
    [TestMethod]
    public void InsertTenantEntity() {
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
  }
}
