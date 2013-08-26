using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouchPotato.CouchClientAdapter;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Test {
  class BulkUpdaterMock : BulkUpdater {

    private List<JObject> entitiesToUpdate;
    private List<BulkResponseRow> responseRows;

    public BulkUpdaterMock() {
      entitiesToUpdate = new List<JObject>();
      responseRows = new List<BulkResponseRow>();
    }

    public void AddMockResponse(BulkResponseRow row) {
      responseRows.Add(row);
    }

    public BulkResponse Execute() {
      return new BulkResponse(responseRows.ToArray());
    }

    public void Update(JObject entity) {
      entitiesToUpdate.Add(entity);
    }

    public List<JObject> EntitiesToUpdate {
      get { return entitiesToUpdate; }
    }

  }
}
