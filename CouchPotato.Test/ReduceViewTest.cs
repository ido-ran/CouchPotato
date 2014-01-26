using System;
using System.Linq;
using CouchPotato.Odm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CouchPotato.Test {
  [TestClass]
  public class ReduceViewTest {

    private class ReduceEntity {
      public string Key { get; set; }
      public int Yes { get; set; }
      public int No { get; set; }
      public int Count { get; set; }
    }

    [TestMethod]
    public void LoadReduceViewToEntities() {
      string rawResponse = @"
{
rows: [
{
key: ""20121010170328-מבנה-ארגוני"",
value: {
yes: 49,
no: 3,
count: 52
}
},
{
key: ""20120821121948-Test2"",
value: {
yes: 4,
no: 4,
count: 8
}
},
{
key: ""20120814132737-שאלון-לבדיקה-1"",
value: {
yes: 14,
no: 66,
count: 80
}
}
]
}";
      var couchDBClientMock = new CouchDBClientAdapterMock(rawResponse);
      CouchDBContextImpl subject = ContextTestHelper.BuildContextForTest(couchDBClientMock);
      ReduceEntity[] results = subject.ReduceView<ReduceEntity>("fake_not_used").ToArray();
      Assert.IsNotNull(results);
      Assert.AreEqual(3, results.Length);
    }
  }
}
