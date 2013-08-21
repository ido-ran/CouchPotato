using System;
using System.Linq;
using CouchPotato.CouchClientAdapter;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Test {
  class CouchDBClientAdapterMock : CouchDBClientAdapter {
    
    private readonly string rawResponse;
    private readonly BulkUpdater bulkUpdater;

    public CouchDBClientAdapterMock(string rawResponse, BulkUpdater bulkUpdater = null) {
      this.rawResponse = rawResponse;
      this.bulkUpdater = bulkUpdater;
    }

    public Newtonsoft.Json.Linq.JToken[] GetViewRows(string viewName, CouchViewOptions viewOptions) {
      JObject parsedResponse = JObject.Parse(rawResponse);
      JArray rows = (JArray)parsedResponse["rows"];
      return rows.ToArray();
    }

    public BulkUpdater CreateBulkUpdater() {
      if (bulkUpdater == null) {
        throw new NotSupportedException("There is no bulk updater mock");
      }
      else {
        return bulkUpdater;
      }
    }
  }
}
