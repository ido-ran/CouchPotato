using System;
using System.Linq;
using CouchPotato.Odm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CouchPotato.Test {
  [TestClass]
  public class CouchDBContextDeleteTest {
    [TestMethod]
    public void DeleteEntityMarkItForDeletion() {
      var couchDBClientMock = new CouchDBClientAdapterMock(SingleUserRawResponse);
      CouchDBContext subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      UserModel user = subject.View<UserModel>("fake_not_used").SingleOrDefault();
      subject.Delete(user);

      DocumentState actual = subject.DocumentManager.DocInfo(user.UserModelID).State;
      DocumentState expected = DocumentState.Delete;

      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void DeleteEntityAddedToBulkUpdater() {
      var bulkUpdaterMock = new BulkUpdaterMock();
      var couchDBClientMock = new CouchDBClientAdapterMock(SingleUserRawResponse, bulkUpdaterMock);
      CouchDBContext subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      UserModel user = subject.View<UserModel>("fake_not_used").SingleOrDefault();
      subject.Delete(user);

      subject.SaveChanges();

      Assert.AreEqual(1, bulkUpdaterMock.EntitiesToDelete.Count);
      Assert.AreEqual("1", bulkUpdaterMock.EntitiesToDelete[0]);
    }

    private const string SingleUserRawResponse = @"
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
  }
}
