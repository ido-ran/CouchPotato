using System.Collections.Generic;
using System.Linq;
using CouchPotato.CouchClientAdapter;
using CouchPotato.Odm;
using LoveSeat;
using Newtonsoft.Json.Linq;

namespace CouchPotato.LoveSeatAdapter {
  /// <summary>
  /// BulkUpdater implementation using LoveSeat client.
  /// </summary>
  internal class LoveSeatBulkUpdater : BulkUpdater {

    private readonly CouchDatabase couchDB;
    private readonly List<JObject> docsToUpdate;
    private readonly bool allOrNothing;

    public LoveSeatBulkUpdater(CouchDatabase couchDB, bool allOrNothing) {
      this.couchDB = couchDB;
      docsToUpdate = new List<JObject>();
      this.allOrNothing = allOrNothing;
    }

    public void Update(JObject entityAsDoc) {
      docsToUpdate.Add(entityAsDoc);
    }

    public void Delete(string id, string rev) {
      JObject docToDel = new JObject();
      docToDel.Add(CouchDBFieldsConst.DocId, id);
      docToDel.Add(CouchDBFieldsConst.DocRev, rev);
      docToDel.Add(CouchDBFieldsConst.DocDelete, true);
      Update(docToDel);
    }

    public BulkResponse Execute() {
      Documents docs = new Documents();
      docs.Values.AddRange(docsToUpdate.Select(x => new Document(x)));

      BulkDocumentResponses bulkResponse = couchDB.SaveDocuments(docs, allOrNothing);
      var abstractResponseRows =
        bulkResponse.Select(x => new BulkResponseRow(x.Id, x.Rev, x.Error, x.Reason))
        .ToArray();

      return new BulkResponse(abstractResponseRows);
    }

  }
}
