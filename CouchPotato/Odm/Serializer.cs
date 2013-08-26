using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json.Linq;
using CouchPotato.Annotations;
using CouchPotato.Odm.Internal;

namespace CouchPotato.Odm {
  /// <summary>
  /// Responsible for serializing and deserializing entities to documents.
  /// </summary>
  internal class Serializer {

    private readonly CouchDBContext context;

    public Serializer(CouchDBContext context) {
      this.context = context;
    }

    /// <summary>
    /// Create proxy from the specified document and entity type.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="entityType"></param>
    /// <returns></returns>
    public object CreateProxy(
      JToken doc, Type entityType, string id, PreProcessInfo preProcess,
      OdmViewProcessingOptions processingOptions, bool emptyProxy) {

      object proxy = Activator.CreateInstance(entityType);
      FillProxy(proxy, doc, id, preProcess, processingOptions, emptyProxy);
      return proxy;
    }

    public JObject Serialize(string rev, string docType, object entity) {
      JObject document = new JObject();

      document.Add(CouchDBFieldsConst.DocType, docType);

      if (rev != null) {
        document.Add(CouchDBFieldsConst.DocRev, rev);
      }

      FillDocument(document, entity);

      return document;
    }

    private void FillDocument(JObject doc, object entity) {
      foreach (PropertyInfo prop in GetPropertiesOf(entity)) {
        if (IsSimpleType(prop)) {
          WriteValueType(entity, prop, doc);
        }
        else if (IsArray(prop)) {
          WriteArray(entity, prop, doc);
        }
        else if (IsReference(prop)) {
          WriteReferenceProxies(entity, prop, doc);
        }
        else {
          Debug.WriteLine(
            string.Format("The property {0}.{1} is of type {2} which is not supported yet",
              prop.DeclaringType.Name, prop.Name, prop.PropertyType.Name));
        }
      }
    }

    private void WriteReferenceProxies(object entity, PropertyInfo prop, JObject doc) {
      AssociationAttribute associationAttr = AssociationAttribute.GetSingle(prop);

      // Serialize assoication collection only for direct association.
      if (associationAttr == null) {
        AssociationCollectionHelper collectionHelper = new AssociationCollectionHelper(entity, prop);
        if (!collectionHelper.IsEmpty) {
          string fieldName = Serializer.GetJsonFieldName(prop);
          object[] associatedIds = collectionHelper.GetIds();
          doc.Add(fieldName, new JArray(associatedIds));
        }
      }
    }

    private void WriteArray(object entity, PropertyInfo prop, JObject doc) {
      string jsonFieldName = GetJsonFieldName(prop);
      object value = prop.GetValue(entity);
      doc.Add(jsonFieldName, new JArray(value));
    }

    private void WriteValueType(object entity, PropertyInfo prop, JObject doc) {
      string fieldName = GetJsonFieldName(prop);
      object value = prop.GetValue(entity);
      doc.Add(fieldName, new JValue(value));
    }

    internal void FillProxy(
      object proxy, JToken doc, string id, PreProcessInfo preProcess,
      OdmViewProcessingOptions processingOptions, bool emptyProxy) {

      foreach (PropertyInfo prop in GetPropertiesOf(proxy)) {
        if (emptyProxy && IsSimpleType(prop)) {
          ReadValueType(proxy, prop, doc);
        }
        else if (emptyProxy && IsArray(prop)) {
          ReadArray(proxy, prop, doc);
        }
        else if (IsReference(prop)) {
          CreateReferenceProxies(proxy, prop, doc, id, preProcess, processingOptions, emptyProxy);
        }
        else {
          Debug.WriteLine(
            string.Format("The property {0}.{1} is of type {2} which is not supported yet",
              prop.DeclaringType.Name, prop.Name, prop.PropertyType.Name));
        }
      }
    }

    private void CreateReferenceProxies(
      object proxy, PropertyInfo prop, JToken doc, string id,
      PreProcessInfo preProcess, OdmViewProcessingOptions processingOptions, bool emptyProxy) {

      AssociationAttribute associationAttr = AssociationAttribute.GetSingle(prop);
      if (associationAttr == null) {
        if (emptyProxy) {
          CreateDirectAssociationCollection(proxy, prop, doc);
        }
      }
      else {
        CreateInverseAssociationCollection(proxy, prop, doc, id, preProcess, associationAttr, processingOptions);
      }
    }

    private void CreateInverseAssociationCollection(
      object proxy, PropertyInfo prop, JToken doc, string id,
      PreProcessInfo preProcess, AssociationAttribute associationAttr,
      OdmViewProcessingOptions processingOptions) {

      object keyPart;
      if (processingOptions.AssoicateCollectionsToLoad.TryGetValue(prop.Name, out keyPart)) {

        Type elementType = prop.PropertyType.GenericTypeArguments[0];

        string[] inverseKeys =
          preProcess.Rows
          .Where(x => x.EntityType.Equals(elementType) && x.Key.MatchRelatedId(id, keyPart))
          .Select(x => x.Id)
          .ToArray();

        SetInverseAssociationCollectionInternal(proxy, prop, associationAttr, inverseKeys);
      }
    }

    internal void SetInverseAssociationCollectionInternal(
      object proxy, PropertyInfo prop, AssociationAttribute associationAttr, string[] inverseKeys) {

      Type elementType = prop.PropertyType.GenericTypeArguments[0];
      Type associateCollectionClosedType = CreateAssociateCollectionType(prop.PropertyType, elementType);

      object associateCollection = Activator.CreateInstance(
        associateCollectionClosedType,
        proxy, inverseKeys, context, associationAttr);

      prop.SetValue(proxy, associateCollection);
    }

    private void CreateDirectAssociationCollection(object proxy, PropertyInfo prop, JToken doc) {
      JArray jArr = GetJArray(prop, doc);
      if (jArr != null) {
        Array clrArr = ResolveArray(typeof(string), jArr);

        SetDirectAssoicationCollectionProperty(proxy, prop, clrArr);
      }
    }

    internal void SetDirectAssoicationCollectionProperty(object proxy, PropertyInfo prop, Array clrArr) {
      Type elementType = prop.PropertyType.GenericTypeArguments[0];
      Type associateCollectionClosedType = CreateAssociateCollectionType(prop.PropertyType, elementType);

      object associateCollection = Activator.CreateInstance(
        associateCollectionClosedType,
        proxy, (string[])clrArr, context, null);

      prop.SetValue(proxy, associateCollection);
    }

    private Type CreateAssociateCollectionType(Type collectionType, Type elementType) {
      Type associateCollectionOpenType;

      if (collectionType.GUID.Equals(typeof(ICollection<>).GUID)) {
        associateCollectionOpenType = typeof(AssociationList<>);
      }
      else if (collectionType.GUID.Equals(typeof(ISet<>).GUID)) {
        associateCollectionOpenType = typeof(AssociationSet<>);
      }
      else {
        throw new NotSupportedException("Association property of unsupported type " + collectionType);
      }

      Type closedType = associateCollectionOpenType.MakeGenericType(elementType);
      return closedType;
    }

    private void ReadArray(object proxy, PropertyInfo prop, JToken doc) {
      Array arr = GetJsonArray(prop, doc);
      if (arr != null) {
        prop.SetValue(proxy, arr);
      }
    }

    /// <summary>
    /// Read Json array from the document according to the prop field name.
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="doc"></param>
    /// <returns></returns>
    internal static Array GetJsonArray(PropertyInfo prop, JToken doc, Type elementType) {
      Array clrArr = null;
      JArray jArray = GetJArray(prop, doc);
      if (jArray != null) {
        clrArr = ResolveArray(elementType, jArray);
      }

      return clrArr;
    }

    private static Array GetJsonArray(PropertyInfo prop, JToken doc) {
      Type elementType = prop.PropertyType.GetElementType();
      return GetJsonArray(prop, doc, elementType);
    }

    private static JArray GetJArray(PropertyInfo prop, JToken doc) {
      string jsonFieldName = GetJsonFieldName(prop);
      var jArr = (JArray)doc[jsonFieldName];
      return jArr;
    }

    private static Array ResolveArray(Type elementType, JArray jArr) {
      Array clrArr = Array.CreateInstance(elementType, jArr.Count);

      for (int index = 0; index < jArr.Count; index++) {
        object value = ResolveValue(jArr[index], elementType);
        clrArr.SetValue(value, index);
      }
      return clrArr;
    }

    private static bool IsReference(PropertyInfo prop) {
      return (typeof(ICollection<>).GUID.Equals(prop.PropertyType.GUID) ||
              typeof(ISet<>).GUID.Equals(prop.PropertyType.GUID));
    }

    private static bool IsArray(PropertyInfo prop) {
      return prop.PropertyType.IsArray;
    }

    private static bool IsSimpleType(PropertyInfo prop) {
      bool isSimple =
        prop.PropertyType.IsValueType ||
        typeof(string) == prop.PropertyType;

      return isSimple;
    }

    private void ReadValueType(object proxy, PropertyInfo prop, JToken doc) {
      try {
        string jsonFieldName = GetJsonFieldName(prop);
        JToken jToken = doc[jsonFieldName];

        object convertedValue = ResolveValue(jToken, prop.PropertyType);
        if (convertedValue != null) {
          prop.SetValue(proxy, convertedValue);
        }
      }
      catch (NotImplementedException ex) {
        throw new InvalidOperationException("Fail to ready value for property " + prop, ex);
      }
    }

    /// <summary>
    /// Convert the inner jToken value to the desired type.
    /// </summary>
    /// <param name="jToken"></param>
    /// <param name="desiredType"></param>
    /// <returns></returns>
    internal static object ResolveValue(JToken jToken, Type desiredType) {
      object convertedValue;

      if (jToken == null) {
        convertedValue = null;
      }
      else {
        JValue jVal = jToken as JValue;
        if (jVal != null) {
          Type nullableUnderlyingType = Nullable.GetUnderlyingType(desiredType);
          if (nullableUnderlyingType != null) {
            convertedValue = Activator.CreateInstance(desiredType, jVal.Value);
          }
          else {
            convertedValue = Convert.ChangeType(jVal.Value, desiredType);
          }
        }
        else {
          throw new NotImplementedException("Unable to handle Json toekn of type " + jToken.GetType().Name);
        }
      }

      return convertedValue;
    }

    internal static string GetJsonFieldName(PropertyInfo prop) {
      string fieldName;
      if (IsKeyField(prop)) {
        fieldName = "_id";
      }
      else if (IsSimpleType(prop) || IsReference(prop) || IsArray(prop)) {
        fieldName = StringUtil.ToCamelCase(prop.Name);
      }
      else if (IsToOneReference(prop)) {
        fieldName = StringUtil.ToCamelCase(prop.Name) + "ID";
      }
      else {
        throw new NotSupportedException("Not supported property info " + prop);
      }

      return fieldName;
    }

    /// <summary>
    /// A property considered ToOne reference if it is a reference type
    /// and singular (not enumerable).
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    private static bool IsToOneReference(PropertyInfo prop) {
      bool isToOneRef =
        !prop.PropertyType.IsValueType &&
        !(typeof(IEnumerable).IsAssignableFrom(prop.PropertyType));

      return isToOneRef;
    }

    private static bool IsKeyField(PropertyInfo prop) {
      bool isKey = (prop.Name.Equals(GetEntityIdPropertyName(prop.DeclaringType), StringComparison.OrdinalIgnoreCase));
      return isKey;
    }

    internal static string GetEntityIdPropertyName(Type entityType) {
      return entityType.Name + "ID";
    }

    internal static PropertyInfo GetEntityIdGetter(Type entityType) {
      PropertyInfo idGetter = entityType.GetProperty(Serializer.GetEntityIdPropertyName(entityType));
      Debug.Assert(idGetter != null, "Fail to find ID getter property for " + entityType);

      return idGetter;
    }

    private IEnumerable<PropertyInfo> GetPropertiesOf(object proxy) {
      return proxy.GetType().GetProperties(
        BindingFlags.FlattenHierarchy |
        BindingFlags.Public |
        BindingFlags.Instance);
    }

  }
}
