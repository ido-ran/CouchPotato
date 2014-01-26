using System;
using CouchPotato.CouchClientAdapter;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  /// <summary>
  /// Delete documents by selecting the document id(s) and revision(s) from a view.
  /// </summary>
  public class DeleteByView {

    private readonly CouchDBContextImpl couchDBContext;
    private readonly string viewName;
    private string key;


    /// <summary>
    /// Only this assembly can create DeleteByView instance.
    /// </summary>
    internal DeleteByView(CouchDBContextImpl couchDBContext, string viewName) {
      this.couchDBContext = couchDBContext;
      this.viewName = viewName;
    }

    /// <summary>
    /// Limit the view selection using key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public DeleteByView Key(string key) {
      this.key = key;
      return this;
    }

    /// <summary>
    /// Execute the delete.
    /// </summary>
    public void Execute() {
      var viewOptions = new CouchViewOptions { 
        Key = new System.Collections.Generic.List<object>(new object[] { key }), 
        IncludeDocs = false 
      };

      JToken[] viewRows = couchDBContext.ClientAdaper.GetViewRows(viewName, viewOptions);
      BulkUpdater bulkUpdater = couchDBContext.ClientAdaper.CreateBulkUpdater(false);
      foreach (JToken viewRow in viewRows) {
        string id = viewRow.Value<string>(CouchDBFieldsConst.ResultRowId);
        // We assume the view contain a field call _rev in the value field.
        string rev = viewRow[CouchDBFieldsConst.ViewValue].Value<string>(CouchDBFieldsConst.DocRev);

        bulkUpdater.Delete(id, rev);
      }

      bulkUpdater.Execute();
    }
  }
}
