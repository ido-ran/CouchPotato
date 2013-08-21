using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CouchPotato.Odm;

namespace CouchPotato.Test {
  /// <summary>
  /// Test for reading data from CouchDBContext
  /// </summary>
  [TestClass]
  public class CouchDBContextReadTest {
    [TestMethod]
    public void LoadExistSingleRow_SingleItemFromView() {
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
      CouchDBContext subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      UserModel user = subject.View<UserModel>("fake_not_used").SingleOrDefault();
      Assert.IsNotNull(user);
    }

    [TestMethod]
    public void ValidateNotSpecifiedFieldAreNullProperties() {
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
      CouchDBContext subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      UserModel user = subject.View<UserModel>("fake_not_used").SingleOrDefault();

      Assert.IsNull(user.FirstName);
    }

    [TestMethod]
    public void LoadNotExistSingleItemFromView() {
      string rawResponse = @"
{
total_rows: 123,
offset: 40,
rows: []
}";
      var couchDBClientMock = new CouchDBClientAdapterMock(rawResponse);
      CouchDBContext subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      UserModel user = subject.View<UserModel>("fake_not_used").Key("yoval.b").SingleOrDefault();
      Assert.IsNull(user);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void LoadExistMultipleResults_SingleItemFromView_Exception() {
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
},
{
  id: ""2"",
  key: ""ido.ran"",
  value: null,
  doc: {
    _id: ""2"",
    _rev: ""1-2edc9b67751f21e58895635c4eb47456"",
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
      CouchDBContext subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      UserModel user = subject.View<UserModel>("fake_not_used").SingleOrDefault();
      Assert.IsNull(user);
    }

    [TestMethod]
    public void LoadUserWithSingleAssociateTenant() {
      string rawResponse = @"
{
total_rows: 248,
offset: 0,
rows: [
{
id: ""1"",
key: [
""1"",
0
],
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
}
]
}";

      var couchDBClientMock = new CouchDBClientAdapterMock(rawResponse);
      CouchDBContext subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      UserModel user = subject.View<UserModel>("fake_not_used").SingleOrDefault();
      TenantModel tenant = user.Tenants.Single();

      Assert.IsNotNull(tenant);
    }

    [TestMethod]
    public void LoadMultipleUserWithSingleAssociateTenant() {
      string rawResponse = @"
{
total_rows: 248,
offset: 0,
rows: [
{
id: ""1"",
key: [
""1"",
0
],
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
id: ""10"",
key: [
""10"",
0
],
value: null,
doc: {
_id: ""10"",
_rev: ""1-a24d2f93d829d6a4901ea2767cf6b67c"",
email: ""shakira@gmail.com"",
password: ""9AC256B12CD5000CBCA395EB15B7872E5C8C3F4E"",
passwordSalt: ""CS0KHNC0ukhG9LzJDh+WI0WZzoyRC5GCwWA6UGJOAbY="",
tenants: [
""20121224110842-shakira""
],
username: ""shakira@gmail.com"",
$type: ""user""
}
},
{
id: ""20121224110842-shakira"",
key: [
""10"",
1
],
value: null,
doc: {
_id: ""20121224110842-shakira"",
_rev: ""1-d786ce65b8694a4c2d3a3b77b43be131"",
defaultLanguage: ""he"",
name: ""shakira"",
planID: ""Free30"",
users: [
""10"",
""26""
],
$type: ""tenant""
}
}
]
}";

      var couchDBClientMock = new CouchDBClientAdapterMock(rawResponse);
      CouchDBContext subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      IEnumerable<UserModel> users = subject.View<UserModel>("fake_not_used");

      Assert.AreEqual(2, users.Count());
    }

    [TestMethod]
    public void ValidateFieldNamesWithTwoWords() {
      string rawResponse = @"
{
total_rows: 248,
offset: 0,
rows: [
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
}
]
}";

      var couchDBClientMock = new CouchDBClientAdapterMock(rawResponse);
      CouchDBContext subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      TenantModel tenant = subject.View<TenantModel>("fake_not_used").SingleOrDefault();

      Assert.AreEqual("he-IL", tenant.DefaultLanguage);
    }

  }
}
