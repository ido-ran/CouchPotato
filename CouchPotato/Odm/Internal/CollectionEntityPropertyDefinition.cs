using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CouchPotato.Annotations;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm.Internal {
  class CollectionEntityPropertyDefinition : EntityPropertyDefinitionBase {

    public CollectionEntityPropertyDefinition(PropertyInfo propInfo)
      : base(propInfo, StringUtil.ToCamelCase(propInfo.Name)) {
    }

    public override void Read(object entity, JToken doc, string id, PreProcessInfo preProcess,
      OdmViewProcessingOptions processingOptions, bool emptyProxy, CouchDBContext context) {
      CreateReferenceProxies(entity, doc, id, preProcess, processingOptions, emptyProxy, context);
    }

    public override void Write(object entity, JObject doc) {
      WriteReferenceProxies(entity, doc);
    }

    private void WriteReferenceProxies(object entity, JObject doc) {
      AssociationAttribute associationAttr = AssociationAttribute.GetSingle(PropertyInfo);

      // Serialize assoication collection only for direct association.
      if (associationAttr == null) {
        AssociationCollectionHelper collectionHelper = new AssociationCollectionHelper(entity, PropertyInfo);
        if (!collectionHelper.IsEmpty) {
          object[] associatedIds = collectionHelper.GetIds();
          doc.Add(JsonFieldName, new JArray(associatedIds));
        }
      }
    }

    private void CreateReferenceProxies(
  object proxy, JToken doc, string id,
  PreProcessInfo preProcess, OdmViewProcessingOptions processingOptions, bool emptyProxy,
      CouchDBContext context) {

      AssociationAttribute associationAttr = AssociationAttribute.GetSingle(PropertyInfo);
      if (associationAttr == null) {
        if (emptyProxy) {
          CreateDirectAssociationCollection(proxy, doc, context);
        }
      }
      else {
        CreateInverseAssociationCollection(proxy, doc, id, preProcess, associationAttr, processingOptions, context);
      }
    }

    private void CreateInverseAssociationCollection(
      object proxy, JToken doc, string id,
      PreProcessInfo preProcess, AssociationAttribute associationAttr,
      OdmViewProcessingOptions processingOptions, CouchDBContext context) {

      object keyPart;
      if (processingOptions.AssoicateCollectionsToLoad.TryGetValue(PropertyInfo.Name, out keyPart)) {

        Type elementType = PropertyInfo.PropertyType.GenericTypeArguments[0];

        string[] inverseKeys =
          preProcess.Rows
          .Where(x => x.EntityType.Equals(elementType) && x.Key.MatchRelatedId(id, keyPart))
          .Select(x => x.Id)
          .ToArray();

        context.Serializer.SetInverseAssociationCollectionInternal(proxy, PropertyInfo, associationAttr, inverseKeys);
      }
    }

    private void CreateDirectAssociationCollection(object proxy, JToken doc, CouchDBContext context) {
      JArray jArr = Serializer.GetJArray(PropertyInfo, doc);
      if (jArr != null) {
        Array clrArr = Serializer.ResolveArray(typeof(string), jArr);

        context.Serializer.SetDirectAssoicationCollectionProperty(proxy, PropertyInfo, clrArr);
      }
    }


  }
}
