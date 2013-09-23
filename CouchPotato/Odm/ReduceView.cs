using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouchPotato.CouchClientAdapter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  /// <summary>
  /// Represent a selection from view that has been reduced.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ReduceView<T> : IEnumerable<T> {

    private readonly CouchDBContext couchDBContext;
    private readonly string viewName;
    private CouchViewOptions options;

    public ReduceView(CouchDBContext couchDBContext, string viewName) {
      this.couchDBContext = couchDBContext;
      this.viewName = viewName;
      options = new CouchViewOptions { Group = true };
    }

    public IEnumerator<T> GetEnumerator() {
      return ExecuteView().GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    private List<T> ExecuteView() {
      JToken[] rows = couchDBContext.ClientAdaper.GetViewRows(viewName, options);

      var results = new List<T>();
      foreach (JToken row in rows) {
        T entity = (T)row["value"].ToObject(typeof(T));
        typeof(T).GetProperty("Key").SetValue(entity, row.Value<string>("key"));
        results.Add(entity);
      }

      return results;
    }

    public ReduceView<T> Keys(string[] keys) {
      if (keys != null) {
        options.Keys.AddRange(keys);
      }
      return this;
    }
  }
}
