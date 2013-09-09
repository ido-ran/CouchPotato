using System.Collections.Generic;
using System.Linq;
using CouchPotato.CouchClientAdapter;
using LoveSeat;
using Newtonsoft.Json.Linq;

namespace CouchPotato.LoveSeatAdapter {
  internal class LoveSeatClientAdapter : CouchDBClientAdapter {

    private readonly CouchDatabase couchDB;

    public LoveSeatClientAdapter(CouchDatabase couchDB) {
      this.couchDB = couchDB;
    }

    public JToken[] GetViewRows(string viewName, CouchViewOptions odmViewOptions) {
      var loveSeatViewOption = ToLoveSeatOptions(odmViewOptions);
      ViewResult viewResult = couchDB.View(viewName, loveSeatViewOption, viewName);
      return viewResult.Rows.ToArray();
    }

    private LoveSeat.ViewOptions ToLoveSeatOptions(CouchViewOptions odmViewOptions) {
      var opt = new LoveSeat.ViewOptions
      {
        Limit = odmViewOptions.Limit,
        IncludeDocs = odmViewOptions.IncludeDocs,
        Keys = odmViewOptions.Keys.Count == 0 ? null : odmViewOptions.Keys.Select(x => new KeyOptions(x)).ToArray()
      };

      CopyKeys(odmViewOptions.Key, opt.Key);
      CopyKeys(odmViewOptions.StartKey, opt.StartKey);
      CopyKeys(odmViewOptions.EndKey, opt.EndKey);
      

      return opt;
    }

    private static void CopyKeys(List<object> odmKeys, LoveSeat.Interfaces.IKeyOptions loveSeatKeys) {
      foreach (object key in odmKeys) {
        loveSeatKeys.Add(key);
      }
    }

    public BulkUpdater CreateBulkUpdater(bool allOrNothing) {
      return new LoveSeatBulkUpdater(couchDB, allOrNothing);
    }

    public JToken[] GetDocuments(string[] ids) {
      var keys = new Keys();
      keys.Values.AddRange(ids);

      ViewResult viewResult = couchDB.GetDocuments(keys);
      return viewResult.Rows.ToArray();
    }

  }
}
