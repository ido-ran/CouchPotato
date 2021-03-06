﻿using System;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  /// <summary>
  /// Represent a pre-process entity information gather from view response.
  /// </summary>
  internal class PreProcessEntityInfo {
    private readonly string id;
    private readonly Type entityType;
    private readonly CouchDBViewRowKey key;
    private readonly JToken row;

    public PreProcessEntityInfo(string id, Type entityType, CouchDBViewRowKey key, JToken row) {
      this.id = id;
      this.entityType = entityType;
      this.key = key;
      this.row = row;
    }

    public string Id { get { return id; } }
    public Type EntityType { get { return entityType; } }
    public CouchDBViewRowKey Key { get { return key; } }
    public JToken Row { get { return row; } }

    public override string ToString() {
      return string.Format("{0} {1} {2}", GetType().Name, entityType.Name, id);
    }
  }

}
