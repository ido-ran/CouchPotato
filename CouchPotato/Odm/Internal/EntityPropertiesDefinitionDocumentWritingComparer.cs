using System;
using System.Collections.Generic;

namespace CouchPotato.Odm.Internal {
  /// <summary>
  /// Compare EntityPropertyDefinition for document writing.
  /// </summary>
  internal class EntityPropertiesDefinitionDocumentWritingComparer : IComparer<EntityPropertyDefinition> {

    private Dictionary<Type, int> orderDic;

    public EntityPropertiesDefinitionDocumentWritingComparer() {
      orderDic = new Dictionary<Type, int>();

      // We want to make sure that ToOne is always get written before
      // value type because entity can have both properties
      // with the same JSON field name.
      orderDic.Add(typeof(ToOneEntityPropertyDefinition), 10);
      orderDic.Add(typeof(ValueTypeEntityPropertyDefinition), 20);
    }

    public int Compare(EntityPropertyDefinition x, EntityPropertyDefinition y) {
      if (object.ReferenceEquals(x, y)) return 0;

      int order = x.JsonFieldName.CompareTo(y.JsonFieldName);
      if (order == 0) {
        // Only if the JSON field names are the same
        // we turn to the order dictionary.
        int orderX = orderDic[x.GetType()];
        int orderY = orderDic[y.GetType()];
        order = orderX - orderY;
      }

      return order;
    }

  }
}
