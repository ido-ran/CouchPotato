using System;
using System.Linq;
using System.Collections.Generic;

namespace CouchPotato.Odm {
  /// <summary>
  /// Manage the document loaded and saved by the context.
  /// </summary>
  internal class DocumentManager {

    /// <summary>
    /// Map document id to document infomation.
    /// </summary>
    private readonly Dictionary<string, CouchDocInfo> docInfos;

    public DocumentManager() {
      docInfos = new Dictionary<string, CouchDocInfo>();
    }

    /// <summary>
    /// Get the document information by document id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CouchDocInfo DocInfo(string id) {
      return docInfos[id];
    }

    internal void Add(List<EntityInfo> entitiesToAdd) {
      foreach (EntityInfo entityInfo in entitiesToAdd) {
        docInfos.Add(entityInfo.IdRev.Id, 
          new CouchDocInfo(entityInfo.IdRev.Id, entityInfo.IdRev.Rev, DocumentState.Clean));
      }
    }

    internal void MarkInserted(string id) {
      docInfos[id] = new CouchDocInfo(id, null, DocumentState.New);
    }

    internal void MarkUpdated(string id) {
      CouchDocInfo docInfo = GetDocInfo(id);
      docInfos[id] = docInfo.ChangeState(DocumentState.Modified);
    }

    internal void MarkClean(string id, string newRev) {
      CouchDocInfo docInfo = GetDocInfo(id);
      docInfos[id] = docInfo.ChangeState(DocumentState.Clean, newRev);
    }

    private CouchDocInfo GetDocInfo(string id) {
      CouchDocInfo docInfo;
      if (!docInfos.TryGetValue(id, out docInfo)) {
        throw new Exception("Fail to find entity information for " + id);
      }
      return docInfo;
    }

    internal CouchDocInfo[] GetModifiedDocuments() {
      var modified =
        from di in docInfos.Values
        where di.State != DocumentState.Clean
        select di;

      return modified.ToArray();
    }

  }
}
