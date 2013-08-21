using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm.Internal {
  internal class AssociationCollectionHelper {

    private readonly object entity;
    private readonly PropertyInfo prop;
    private readonly IEnumerable typelessCollection;
    private readonly AssociationCollection assocColl;

    public AssociationCollectionHelper(object entity, PropertyInfo prop) {
      this.entity = entity;
      this.prop = prop;
      typelessCollection = (IEnumerable)prop.GetValue(entity);
      assocColl = typelessCollection as AssociationCollection;
    }

    public bool IsEmpty {
      get { return 0 == Count; }
    }

    private int Count {
      get {
        if (assocColl != null) {
          return assocColl.Count;
        }
        else {
          PropertyInfo countProp = typelessCollection.GetType().GetProperty("Count");
          return (int)countProp.GetValue(typelessCollection);
        }
      }
    }

    internal object[] GetIds() {
      if (assocColl != null) {
        return assocColl.GetEntityIds();
      }
      else {
        PropertyInfo idGetter = GetIdGetter();
        return GetCollectionAsObject().Select(x => idGetter.GetValue(x)).ToArray();
      }
    }

    private PropertyInfo GetIdGetter() {
      // Get collection's entity ID getter.
      Type elementType = typelessCollection.GetType().GetGenericArguments()[0];
      return Serializer.GetEntityIdGetter(elementType);
    }

    /// <summary>
    /// Convert the ICollection<T> to T[]
    /// </summary>
    /// <returns></returns>
    private object[] GetCollectionAsObject() {
      object[] collection = typelessCollection.Cast<object>().ToArray();
      return collection;
    }

    /// <summary>
    /// Call the Remove method of the underlying collection.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    internal bool Remove(object item) {
      MethodInfo removeMethod = typelessCollection.GetType().GetMethod("Remove");
      bool result = (bool) removeMethod.Invoke(typelessCollection, new [] { item });
      return result;
    }

    /// <summary>
    /// Call the Add method of the underlying collection.
    /// </summary>
    /// <param name="item"></param>
    internal void Add(object item) {
      MethodInfo addMethod = typelessCollection.GetType().GetMethod("Add");
      addMethod.Invoke(typelessCollection, new[] { item });
    }
  }
}
