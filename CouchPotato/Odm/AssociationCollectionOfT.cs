﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using CouchPotato.Annotations;
using CouchPotato.Odm.Internal;

namespace CouchPotato.Odm {
  /// <summary>
  /// Represent the association collection used to model relations between entities.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal abstract class AssociationCollection<T> : AssociationCollection, ICollection<T> {

    /// <summary>
    /// Hold the association collection owner instance.
    /// </summary>
    private readonly object owner;

    /// <summary>
    /// Reference to the CouchDBContext.
    /// </summary>
    protected readonly CouchDBContextImpl context;

    /// <summary>
    /// Hold Association attribute if one is present on the association property.
    /// </summary>
    private readonly AssociationAttribute associationAttr;

    public AssociationCollection(
      object owner,
      CouchDBContextImpl context, AssociationAttribute associationAttr) {

      this.owner = owner;
      this.context = context;
      this.associationAttr = associationAttr;
    }

    public void Add(T item) {
      if (associationAttr == null) {
        AddInternal(item);
      }
      else {
        context.Update(item);

        AssociationCollectionHelper associationCollHelper = GetInverseCollectionHelper(item);
        associationCollHelper.Add(owner);
      }
    }

    protected abstract void AddInternal(T item);

    public void Clear() {
      if (associationAttr == null) {
        ResetModificationCollection();
      }
      else {
        // In order to clear inverse association we must materialize
        // the entities of this collection and remove each one
        // to update the inverse side.
        var allItems = this.ToList();
        foreach (T item in allItems) {
          Remove(item);
        }
      }
    }

    protected abstract void ResetModificationCollection();

    public bool Contains(T item) {
      throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex) {
      int index = arrayIndex;
      foreach (T item in ActiveCollection) {
        array[index++] = item;
      }
    }

    public abstract int Count {
      get;
    }

    public bool IsReadOnly {
      get { throw new NotImplementedException(); }
    }

    public bool Remove(T item) {
      if (associationAttr == null) {
        // Direct association
        return RemoveInternal(item);
      }
      else {
        // Inverse association

        // Mark the other item as updated.
        // NOTE: this is a bit problematic because it behave different than direct
        // association modifiation which require call to context.Update manually.
        context.Update(item);
        return RemoveInverseAssociation(item);
      }
    }

    protected abstract bool RemoveInternal(T item);

    private bool RemoveInverseAssociation(T item) {
      AssociationCollectionHelper associationCollHelper = GetInverseCollectionHelper(item);
      return associationCollHelper.Remove(owner);
    }

    private AssociationCollectionHelper GetInverseCollectionHelper(T item) {
      PropertyInfo inversePropInfo = item.GetType().GetProperty(associationAttr.InversePropertyName);
      Debug.Assert(inversePropInfo != null, "Fail to find inverse property " + associationAttr.InversePropertyName);
      var associationCollHelper = new AssociationCollectionHelper(item, inversePropInfo);
      return associationCollHelper;
    }

    public IEnumerator<T> GetEnumerator() {
      return ActiveCollection.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    private IEnumerable<T> ActiveCollection {
      get {
        return context.IdentityMap.GetEntities<T>(typeof(T), GetEntityIds());
      }
    }

    public abstract string[] GetEntityIds();

  }
}
