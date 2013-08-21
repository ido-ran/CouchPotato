using System.Collections.Generic;
using System.Linq;
using CouchPotato.CouchClientAdapter;
using LoveSeat;
using Newtonsoft.Json.Linq;

namespace CouchPotato.LoveSeatAdapter {
  /// <summary>
  /// BulkUpdater implementation using LoveSeat client.
  /// </summary>
  internal class LoveSeatBulkUpdater : BulkUpdater {

    private readonly CouchDatabase couchDB;
    private readonly List<JObject> docsToUpdate;

    public LoveSeatBulkUpdater(CouchDatabase couchDB) {
      this.couchDB = couchDB;
      docsToUpdate = new List<JObject>();
    }

    public void Update(JObject entityAsDoc) {
      docsToUpdate.Add(entityAsDoc);
    }

    public BulkResponse Execute() {
      Documents docs = new Documents();
      docs.Values.AddRange(docsToUpdate.Select(x => new Document(x)));

      BulkDocumentResponses bulkResponse = couchDB.SaveDocuments(docs, true);
      var abstractResponseRows =
        bulkResponse.Select(x => new BulkResponseRow(x.Id, x.Rev, x.Error, x.Reason))
        .ToArray();

      return new BulkResponse(abstractResponseRows);
    }

  }
}
