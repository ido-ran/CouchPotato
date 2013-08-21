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
        IncludeDocs = odmViewOptions.IncludeDocs
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

    public BulkUpdater CreateBulkUpdater() {
      return new LoveSeatBulkUpdater(couchDB);
    }

  }
}
