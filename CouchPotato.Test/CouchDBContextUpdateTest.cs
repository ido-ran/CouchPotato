using System;
using System.Collections.Generic;
using System.Linq;
using CouchPotato.CouchClientAdapter;
using CouchPotato.Odm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CouchPotato.Test {
  [TestClass]
  public class CouchDBContextUpdateTest {
    [TestMethod]
    public void DocumentManagerSaveDocumentRevisionOfCleanDocument() {
      CouchDocInfo docInfo = GetCleanDocInfo();
      Assert.AreEqual("1-1edc9b67751f21e58895635c4eb47456", docInfo.Rev);
    }

    [TestMethod]
    public void DocumentInfoOfCleanDocument_IsClean() {
      CouchDocInfo docInfo = GetCleanDocInfo();
      Assert.AreEqual(DocumentState.Clean, docInfo.State);
    }

    private static CouchDocInfo GetCleanDocInfo() {
      string rawResponse = @"
{
total_rows: 123,
offset: 40,
rows: [
{
  id: ""1"",
  key: ""ido.ran"",
  value: null,
  doc: {
    _id: ""1"",
    _rev: ""1-1edc9b67751f21e58895635c4eb47456"",
    email: ""ido.ran@gmail.com"",
    password: ""AAABBBCCC"",
    passwordSalt: ""123123123"",
    roles: [
      ""Admin""
    ],
    tenants: [
      ""20130722094352-TenantA""
    ],
    username: ""ido.ran"",
    $type: ""user""
  }
}
]
}";
      var couchDBClientMock = new CouchDBClientAdapterMock(rawResponse);
      CouchDBContextImpl subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      UserModel userToUpdate = subject.View<UserModel>("fake_not_used").SingleOrDefault();

      CouchDocInfo docInfo = subject.DocumentManager.DocInfo("1");
      return docInfo;
    }

    [TestMethod]
    public void UpdateSingleEntity_StateIsModified() {
      CouchDBContextImpl subject;
      UserModel userToUpdate;
      UpdateSingleDocument(out subject, out userToUpdate);

      subject.Update(userToUpdate);
      CouchDocInfo docInfo = subject.DocumentManager.DocInfo("1");

      Assert.AreEqual(DocumentState.Modified, docInfo.State);
    }

    [TestMethod]
    public void UpdateSingleEntityAndSaveChanges_StateIsClear() {
      CouchDBContextImpl subject;
      UserModel userToUpdate;
      UpdateSingleDocument(out subject, out userToUpdate);

      subject.Update(userToUpdate);
      subject.SaveChanges();

      CouchDocInfo docInfo = subject.DocumentManager.DocInfo("1");
      Assert.AreEqual(DocumentState.Clean, docInfo.State);
    }

    [TestMethod]
    public void UpdateSingleEntityAndSaveChanges_RevisionUpdates() {
      CouchDBContextImpl subject;
      UserModel userToUpdate;
      UpdateSingleDocument(out subject, out userToUpdate);

      subject.Update(userToUpdate);
      subject.SaveChanges();

      CouchDocInfo docInfo = subject.DocumentManager.DocInfo("1");
      Assert.AreEqual("2-123-Updated", docInfo.Rev);
    }

    [TestMethod]
    public void UpdateAssoicatedCollection_RemoveEntityFromCollection_SlaveSide_ValidateOtherCollectionUpdated() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      bulkUpdaterMock.AddMockResponse(new BulkResponseRow("1", "2-123-Updated", null, null));

      var couchDBClientMock = new CouchDBClientAdapterMock(RawResponseWithOneTenant, bulkUpdaterMock);
      var subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      var userToUpdate = subject.View<UserModel>("fake_not_used")
        .AssociatedCollection(x => x.Tenants, 1)
        .SingleOrDefault();

      TenantModel tenantToRemove = userToUpdate.Tenants.Single();
      userToUpdate.Tenants.Remove(tenantToRemove);

      subject.SaveChanges();

      Assert.AreEqual(1, tenantToRemove.Users.Count);
    }

    [TestMethod]
    public void UpdateAssoicatedCollection_RemoveEntityFromCollection_SlaveSide_ValidateOtherEntityIsUpdated() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      bulkUpdaterMock.AddMockResponse(new BulkResponseRow("1", "2-123-Updated", null, null));

      var couchDBClientMock = new CouchDBClientAdapterMock(RawResponseWithOneTenant, bulkUpdaterMock);
      var subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      var userToUpdate = subject.View<UserModel>("fake_not_used")
        .AssociatedCollection(x => x.Tenants, 1)
        .SingleOrDefault();

      TenantModel tenantToRemove = userToUpdate.Tenants.Single();
      userToUpdate.Tenants.Remove(tenantToRemove);

      CouchPotatoAssert.ModifiedDocumentsCount(subject, 1);
    }

    [TestMethod]
    public void UpdateAssoicatedCollection_ClearCollection_SlaveSide_ValidateOtherEntityIsUpdated() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      bulkUpdaterMock.AddMockResponse(new BulkResponseRow("1", "2-123-Updated", null, null));

      var couchDBClientMock = new CouchDBClientAdapterMock(RawResponseWithOneTenant, bulkUpdaterMock);
      var subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      var userToUpdate = subject.View<UserModel>("fake_not_used")
        .AssociatedCollection(x => x.Tenants, 1)
        .SingleOrDefault();

      TenantModel tenantToRemove = userToUpdate.Tenants.Single();
      userToUpdate.Tenants.Clear();

      CouchPotatoAssert.ModifiedDocumentsCount(subject, 1);
    }

    [TestMethod]
    public void UpdateAssoicatedCollection_AddNewEntity_SlaveSide_ValidateOtherEntityIsUpdated() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      bulkUpdaterMock.AddMockResponse(new BulkResponseRow("1", "2-123-Updated", null, null));

      var couchDBClientMock = new CouchDBClientAdapterMock(RawResponseWithOneTenant, bulkUpdaterMock);
      var subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      var userToUpdate = subject.View<UserModel>("fake_not_used")
        .AssociatedCollection(x => x.Tenants, 1)
        .SingleOrDefault();

      TenantModel newTenant = new TenantModel
      {
        TenantModelID = "2001",
        Name = "Tin-Tenant",
        Users = new HashSet<UserModel>()
      };

      subject.Update(newTenant);
      userToUpdate.Tenants.Add(newTenant);

      CouchPotatoAssert.ModifiedDocumentsCount(subject, 1, "Only the new tenant document should be updated (created)");
    }

    [TestMethod]
    public void UpdateAssoicatedCollection_ClearAndReAddOneOfTwoEntities_SlaveSide_ValidateOtherEntityIsUpdated() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      bulkUpdaterMock.AddMockResponse(new BulkResponseRow("1", "2-123-Updated", null, null));

      var couchDBClientMock = new CouchDBClientAdapterMock(RawResponseWithTwoTenants, bulkUpdaterMock);
      var subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      var userToUpdate = subject.View<UserModel>("fake_not_used")
        .AssociatedCollection(x => x.Tenants, 1)
        .SingleOrDefault();

      var firstTenant = userToUpdate.Tenants.First();
      var secondTenant = userToUpdate.Tenants.Skip(1).First();

      // Clear and readd the first tenant
      userToUpdate.Tenants.Clear();
      userToUpdate.Tenants.Add(firstTenant);

      // Update the user even though it should have no effect
      subject.Update(userToUpdate);

      // TODO: it realy should be 1 because only TenantB is modified
      // by removing User 1 from it users array.
      int expectedModifiedDocuments = 3;
      CouchPotatoAssert.ModifiedDocumentsCount(subject, expectedModifiedDocuments, "Only the second tenant should be updated");
    }

    private static void UpdateSingleDocument(out CouchDBContextImpl subject, out UserModel userToUpdate) {
      PrepareForUpdate(out subject, out userToUpdate);
      userToUpdate.FirstName = "Yotam";
    }

    private static void PrepareForUpdate(out CouchDBContextImpl subject, out UserModel userToUpdate) {
      var bulkUpdaterMock = new BulkUpdaterMock();
      bulkUpdaterMock.AddMockResponse(new BulkResponseRow("1", "2-123-Updated", null, null));

      var couchDBClientMock = new CouchDBClientAdapterMock(RawResponseWithOneTenant, bulkUpdaterMock);
      subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      userToUpdate = subject.View<UserModel>("fake_not_used").SingleOrDefault();
    }

    private const string RawResponseWithOneTenant = @"
{
total_rows: 123,
offset: 40,
rows: [
{
  id: ""1"",
  key: ""ido.ran"",
  value: null,
  doc: {
    _id: ""1"",
    _rev: ""1-1edc9b67751f21e58895635c4eb47456"",
    email: ""ido.ran@gmail.com"",
    password: ""AAABBBCCC"",
    passwordSalt: ""123123123"",
    roles: [
      ""Admin""
    ],
    tenants: [
      ""20130722094352-TenantA""
    ],
    username: ""ido.ran"",
    $type: ""user""
  }
},
{
  id: ""20130722094352-TenantA"",
  key: [
  ""1"",
  1
  ],
  value: null,
  doc: {
    _id: ""20130722094352-TenantA"",
    _rev: ""1-9aa586de15bb6784b48600741a8bf841"",
    defaultLanguage: ""he-IL"",
    name: ""Tenant-A"",
    planID: ""Free30"",
    users: [
    ""1"", ""100-not-exist-in-view""
    ],
    $type: ""tenant""
  }
}
]
}";

    private const string RawResponseWithTwoTenants = @"
{
total_rows: 123,
offset: 40,
rows: [
{
  id: ""1"",
  key: ""ido.ran"",
  value: null,
  doc: {
    _id: ""1"",
    _rev: ""1-1edc9b67751f21e58895635c4eb47456"",
    email: ""ido.ran@gmail.com"",
    password: ""AAABBBCCC"",
    passwordSalt: ""123123123"",
    roles: [
      ""Admin""
    ],
    tenants: [
      ""20130722094352-TenantA""
    ],
    username: ""ido.ran"",
    $type: ""user""
  }
},
{
  id: ""20130722094352-TenantA"",
  key: [
  ""1"",
  1
  ],
  value: null,
  doc: {
    _id: ""20130722094352-TenantA"",
    _rev: ""1-9aa586de15bb6784b48600741a8bf841"",
    defaultLanguage: ""he-IL"",
    name: ""Tenant-A"",
    planID: ""Free30"",
    users: [
    ""1""
    ],
    $type: ""tenant""
  }
},
{
  id: ""20130722094353-TenantB"",
  key: [
  ""1"",
  1
  ],
  value: null,
  doc: {
    _id: ""20130722094353-TenantB"",
    _rev: ""1-abc"",
    defaultLanguage: ""he-IL"",
    name: ""Tenant-B"",
    planID: ""Free30"",
    users: [
    ""1""
    ],
    $type: ""tenant""
  }
}
]
}";
  }
}
