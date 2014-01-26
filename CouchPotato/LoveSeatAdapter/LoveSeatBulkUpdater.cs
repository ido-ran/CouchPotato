using System;
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

    /// <summary>
    /// Update at most 500 docs at a time when updating.
    /// If more docs than that need to be updated they will be
    /// divided into chunks and each chunk will be sent separately.
    /// </summary>
    private const int BulkChunkSize = 500;

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
      // This method divid the update to chunks because services such as Cloudant
      // recommend to limit the number of bulk documents to around 500 docs.

      List<BulkResponseRow> responses = new List<BulkResponseRow>(docsToUpdate.Count);

      foreach (JObject[] updateChunk in docsToUpdate.Chunks(BulkChunkSize)) {
        Documents docs = new Documents();
        docs.Values.AddRange(updateChunk.Select(x => new Document(x)));

        BulkDocumentResponses bulkResponse = couchDB.SaveDocuments(docs, allOrNothing);
        IEnumerable<BulkResponseRow> abstractResponseRows =
          bulkResponse.Select(x => new BulkResponseRow(x.Id, x.Rev, x.Error, x.Reason));

        responses.AddRange(abstractResponseRows);
      }


      return new BulkResponse(responses.ToArray());
    }

  }
}
