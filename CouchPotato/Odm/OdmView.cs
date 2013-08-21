using System.Collections.Generic;
using System.Linq;
using CouchPotato.CouchClientAdapter;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  public class OdmView<T> : IEnumerable<T> {
    private readonly CouchDBContext couchDBContext;
    private readonly string viewName;
    private CouchViewOptions options;

    public OdmView(CouchDBContext couchDBContext, string viewName) {
      this.couchDBContext = couchDBContext;
      this.viewName = viewName;
      options = new CouchViewOptions
      {
        IncludeDocs = true
      };
    }

    public OdmView<T> Key(object key) {
      options.Key.Add(key);
      return this;
    }

    public OdmView<T> StartKey(params object[] values) {
      options.StartKey = new List<object>(values);
      return this;
    }

    public OdmView<T> EndKey(params object[] values) {
      options.EndKey = new List<object>(values);
      return this;
    }

    public IEnumerator<T> GetEnumerator() {
      return ExecuteView(options).Entities.OfType<T>().GetEnumerator();
    }

    private EntitiesProcessResult ExecuteView(CouchViewOptions viewOptions) {
      JToken[] rows = couchDBContext.ClientAdaper.GetViewRows(viewName, viewOptions);
      EntitiesProcessResult processResult = couchDBContext.Process(rows);
      return processResult;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}
