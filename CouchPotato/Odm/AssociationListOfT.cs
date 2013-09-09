using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouchPotato.Annotations;

namespace CouchPotato.Odm {
  /// <summary>
  /// Represent association with another entity with list semantics.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal class AssociationList<T> : AssociationCollection<T> {

    /// <summary>
    /// The keys loaded from CouchDB document represent the associated entities.
    /// </summary>
    private readonly string[] keys;


    /// <summary>
    /// Hold list of keys after the collection has been modified.
    /// </summary>
    private List<string> modifiedCollection;

    public AssociationList(
      object owner, string[] keys,
      CouchDBContext context, AssociationAttribute associationAttr)
    : base(owner, context, associationAttr) {

      this.keys = keys;
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
      modifiedCollection = new List<string>(keys);
    }


    public override int Count {
      get { return modifiedCollection != null ? modifiedCollection.Count : keys.Length; }
    }

    public override string[] GetEntityIds() {
      return modifiedCollection != null ? modifiedCollection.ToArray() : keys;
    }
  }
}
