using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouchPotato.Annotations;

namespace CouchPotato.Odm {
  /// <summary>
  /// Represent association with another entity with set semantics.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal class AssociationSet<T> : AssociationCollection<T>, ISet<T> {

    /// <summary>
    /// The keys loaded from CouchDB document represent the associated entities.
    /// </summary>
    private readonly HashSet<string> keys;


    /// <summary>
    /// Hold list of keys after the collection has been modified.
    /// </summary>
    private HashSet<string> modifiedCollection;

    public AssociationSet(
      object owner, string[] keys,
      CouchDBContext context, AssociationAttribute associationAttr)
      : base(owner, context, associationAttr) {

      this.keys = new HashSet<string>(keys);
    }

    protected override void AddInternal(T item) {
      MaterializeModifiedCollection();
      string id = CouchDBContext.GetEntityInstanceId(item);
      modifiedCollection.Add(id);
    }

    protected override bool RemoveInternal(T item) {
      MaterializeModifiedCollection();
      string id = CouchDBContext.GetEntityInstanceId(item);
      return modifiedCollection.Remove(id);
    }

    private void MaterializeModifiedCollection() {
      if (modifiedCollection == null) {
        // First modification
        ResetModificationCollection();
      }
    }

    protected override void ResetModificationCollection() {
      modifiedCollection = new HashSet<string>(keys);
    }


    public override int Count {
      get { return modifiedCollection != null ? modifiedCollection.Count : keys.Count; }
    }


    public override string[] GetEntityIds() {
      return modifiedCollection != null ? modifiedCollection.ToArray() : keys.ToArray();
    }

    #region ISet<T> Members

    public new bool Add(T item) {
      base.Add(item);
      return true;
    }

    public void ExceptWith(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    public void IntersectWith(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    public bool IsProperSubsetOf(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    public bool IsProperSupersetOf(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    public bool IsSubsetOf(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    public bool IsSupersetOf(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    public bool Overlaps(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    public bool SetEquals(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    public void SymmetricExceptWith(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    public void UnionWith(IEnumerable<T> other) {
      throw new NotImplementedException();
    }

    #endregion

    #region IEnumerable Members

    public new System.Collections.IEnumerator GetEnumerator() {
      throw new NotImplementedException();
    }

    #endregion
  }
}
