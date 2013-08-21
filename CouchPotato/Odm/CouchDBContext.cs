using System;
using System.Linq;
using System.Reflection;
using CouchPotato.CouchClientAdapter;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  /// <summary>
  /// Entry point to CouchPotato API.
  /// </summary>
  public class CouchDBContext {

    private readonly CouchDBClientAdapter clientAdapter;
    private readonly IdentityMap identityMap;
    private readonly Mapping mapping;
    private readonly DocumentManager docMgr;

    public CouchDBContext(CouchDBClientAdapter clientAdapter) {
      this.clientAdapter = clientAdapter;
      mapping = new Mapping();
      identityMap = new IdentityMap(this);
      docMgr = new DocumentManager();
    }

    public OdmView<T> View<T>(string viewName) {
      return new OdmView<T>(this, viewName);
    }

    public CouchDBClientAdapter ClientAdaper {
      get { return clientAdapter; }
    }

    public Mapping Mapping {
      get { return mapping; }
    }

    /// <summary>
    /// Mark an entity as updated.
    /// </summary>
    /// <param name="entity"></param>
    public void Update(object entity) {
      string id = IdentityMap.GetIdByEntity(entity);
      if (null == id) {
        // New entity
        DocumentManager.MarkInserted(GetEntityInstanceId(entity));
      }
      else {
        // Update managed entity.
        DocumentManager.MarkUpdated(id);
      }
    }

    internal string GetEntityInstanceId(object entity) {
      PropertyInfo idGetter = Serializer.GetEntityIdGetter(entity.GetType());
      return (string)idGetter.GetValue(entity);
    }

    /// <summary>
    /// Commit changes made in the context to the database.
    /// </summary>
    public void SaveChanges() {

      Tuple<CouchDocInfo, object>[] modifiedEntities = GetModifiedEntities();
      if (modifiedEntities.Length > 0) {
        BulkUpdater bulkUpdater = clientAdapter.CreateBulkUpdater();

        foreach (Tuple<CouchDocInfo, object> entityWithInfo in modifiedEntities) {
          JObject serializedDoc = IdentityMap.EntityAsDocument(entityWithInfo.Item1.Rev, entityWithInfo.Item2);
          bulkUpdater.Update(serializedDoc);
        }

        BulkResponse response = bulkUpdater.Execute();

        foreach (BulkResponseRow row in response.Rows) {
          docMgr.MarkClean(row.Id, row.Rev);
        }
      }
    }

    private Tuple<CouchDocInfo, object>[] GetModifiedEntities() {
      CouchDocInfo[] modifiedDocInfos = DocumentManager.GetModifiedDocuments();
      var modifiedEntities =
          from di in modifiedDocInfos
          select Tuple.Create(di, IdentityMap.GetEntityById(di.Id));

      return modifiedEntities.ToArray();
    }

    internal IdentityMap IdentityMap {
      get { return identityMap; }
    }

    internal DocumentManager DocumentManager {
      get { return docMgr; }
    }

    /// <summary>
    /// Process document rows.
    /// </summary>
    /// <param name="rows"></param>
    /// <returns></returns>
    internal EntitiesProcessResult Process(JToken[] rows) {
      EntitiesProcessResult result = IdentityMap.Process(rows);

      DocumentManager.Add(result.NewEntities);

      return result;
    }
  }
}
