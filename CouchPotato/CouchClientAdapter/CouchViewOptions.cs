using System;
using System.Collections.Generic;

namespace CouchPotato.CouchClientAdapter {
  /// <summary>
  /// Contain CouchDB view options.
  /// </summary>
  public class CouchViewOptions {
    public Nullable<int> Limit { get; set; }
    public bool IncludeDocs { get; set; }
    public List<object> Key { get; set; }
    public List<object> StartKey { get; set; }
    public List<object> EndKey { get; set; }

    public CouchViewOptions() {
      Key = new List<object>();
      StartKey = new List<object>();
      EndKey = new List<object>();
    }
  }
}
