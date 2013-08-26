using System;
using System.Collections.Generic;
using System.Linq;
using CouchPotato.CouchClientAdapter;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Test {
  class CouchDBClientAdapterMock : CouchDBClientAdapter {
    
    private readonly string rawResponse;
    private readonly Queue<string> getDocumentsResponses;
    private readonly BulkUpdater bulkUpdater;

    public CouchDBClientAdapterMock(string rawResponse, BulkUpdater bulkUpdater = null) {
      this.rawResponse = rawResponse;
      this.bulkUpdater = bulkUpdater;
      getDocumentsResponses = new Queue<string>();
    }

    public void AddGetDocumentResponse(string response) {
      getDocumentsResponses.Enqueue(response);
    }

    public Newtonsoft.Json.Linq.JToken[] GetViewRows(string viewName, CouchViewOptions viewOptions) {
      return ParseViewResponse(rawResponse);
    }

    private JToken[] ParseViewResponse(string response) {
      JObject parsedResponse = JObject.Parse(response);
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

    public JToken[] GetDocuments(string[] ids) {
      if (getDocumentsResponses.Count == 0) throw new Exception("No response was prepared");
      string response = getDocumentsResponses.Dequeue();
      return ParseViewResponse(response);
    }

  }
}
