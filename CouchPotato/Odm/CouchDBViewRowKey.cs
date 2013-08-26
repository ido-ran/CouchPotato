using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm {
  /// <summary>
  /// Represent a key in CouchDB view.
  /// Key can be null, simple value, array or JSON object.
  /// </summary>
  internal class CouchDBViewRowKey {
    private readonly object key;

    public CouchDBViewRowKey(object key) {
      this.key = key;
    }

    /// <summary>
    /// Get the raw key as captured from CouchDB view.
    /// </summary>
    public object RawKey {
      get { return key; }
    }

    /// <summary>
    /// Check if the key match the specified related id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="keyPart"></param>
    /// <returns></returns>
    /// <remarks>
    /// This method assume the key is array of structure [{id},0-for main entity, 1-for related entity].
    /// </remarks>
    internal bool MatchRelatedId(string id, object keyPart) {
      object[] joinKey = (object[])key;
      return id.Equals(joinKey[0]) && keyPart.Equals(joinKey[1]);
    }
  }
}
