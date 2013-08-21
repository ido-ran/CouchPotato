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

    internal const string DocTypeField = "$type";
    internal const string RevisionField = "_rev";

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
    public object CreateProxy(JToken doc, Type entityType, string id, PreProcessInfo preProcess) {
      object proxy = Activator.CreateInstance(entityType);
      FillProxy(proxy, doc, id, preProcess);
      return proxy;
    }

    public JObject Serialize(string rev, string docType, object entity) {
      JObject document = new JObject();

      document.Add(DocTypeField, docType);
      document.Add(RevisionField, rev);
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
      AssociationCollectionHelper collectionHelper = new AssociationCollectionHelper(entity, prop);
      if (!collectionHelper.IsEmpty) {
        string fieldName = Serializer.GetJsonFieldName(prop);
        object[] associatedIds = collectionHelper.GetIds();
        doc.Add(fieldName, new JArray(associatedIds));
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

    internal void FillProxy(object proxy, JToken doc, string id, PreProcessInfo preProcess) {
      foreach (PropertyInfo prop in GetPropertiesOf(proxy)) {
        if (IsSimpleType(prop)) {
          ReadValueType(proxy, prop, doc);
        }
        else if (IsArray(prop)) {
          ReadArray(proxy, prop, doc);
        }
        else if (IsReference(prop)) {
          CreateReferenceProxies(proxy, prop, doc, id, preProcess);
        }
        else {
          Debug.WriteLine(
            string.Format("The property {0}.{1} is of type {2} which is not supported yet",
              prop.DeclaringType.Name, prop.Name, prop.PropertyType.Name));
        }
      }
    }

    private void CreateReferenceProxies(
      object proxy, PropertyInfo prop, JToken doc, string id, PreProcessInfo preProcess) {
      AssociationAttribute associationAttr = AssociationAttribute.GetSingle(prop);
      if (associationAttr == null) {
        CreateDirectAssociationCollection(proxy, prop, doc);
      }
      else {
        CreateInverseAssociationCollection(proxy, prop, doc, id, preProcess, associationAttr);
      }
    }

    private void CreateInverseAssociationCollection(
      object proxy, PropertyInfo prop, JToken doc, string id, 
      PreProcessInfo preProcess, AssociationAttribute associationAttr) {

      Type elementType = prop.PropertyType.GenericTypeArguments[0];
      Type associateCollectionClosedType = CreateDirectAssociateCollectionType(elementType);
      string[] inverseKeys = 
        preProcess.Rows
        .Where(x => x.EntityType.Equals(elementType) && x.Key.MatchRelatedId(id))
        .Select(x => x.Id)
        .ToArray();

      object associateCollection = Activator.CreateInstance(
        associateCollectionClosedType,
        proxy, inverseKeys, context, associationAttr);

      prop.SetValue(proxy, associateCollection);
    }

    private void CreateDirectAssociationCollection(object proxy, PropertyInfo prop, JToken doc) {
      JArray jArr = GetJArray(prop, doc);
      if (jArr != null) {
        Type elementType = prop.PropertyType.GenericTypeArguments[0];
        Array clrArr = ResolveArray(typeof(string), jArr);
        Type associateCollectionClosedType = CreateDirectAssociateCollectionType(elementType);

        object associateCollection = Activator.CreateInstance(
          associateCollectionClosedType,
          proxy, (string[])clrArr, context, null);

        prop.SetValue(proxy, associateCollection);
      }
    }

    private Type CreateDirectAssociateCollectionType(Type elementType) {
      Type associateCollectionOpenType = typeof(AssociationCollection<>);
      Type closedType = associateCollectionOpenType.MakeGenericType(elementType);
      return closedType;
    }

    private void ReadArray(object proxy, PropertyInfo prop, JToken doc) {
      var jArr = GetJArray(prop, doc);
      if (jArr != null) {
        Type elementType = prop.PropertyType.GetElementType();
        Array clrArr = ResolveArray(elementType, jArr);

        prop.SetValue(proxy, clrArr);
      }
    }

    private JArray GetJArray(PropertyInfo prop, JToken doc) {
      string jsonFieldName = GetJsonFieldName(prop);
      var jArr = (JArray)doc[jsonFieldName];
      return jArr;
    }

    private Array ResolveArray(Type elementType, JArray jArr) {
      Array clrArr = Array.CreateInstance(elementType, jArr.Count);

      for (int index = 0; index < jArr.Count; index++) {
        object value = ResolveValue(jArr[index], elementType);
        clrArr.SetValue(value, index);
      }
      return clrArr;
    }

    private bool IsReference(PropertyInfo prop) {
      return (typeof(ICollection<>).GUID.Equals(prop.PropertyType.GUID));
    }

    private bool IsArray(PropertyInfo prop) {
      return prop.PropertyType.IsArray;
    }

    private bool IsSimpleType(PropertyInfo prop) {
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

    private object ResolveValue(JToken jToken, Type type) {
      object convertedValue;

      if (jToken == null) {
        convertedValue = null;
      }
      else {
        JValue jVal = jToken as JValue;
        if (jVal != null) {
          convertedValue = Convert.ChangeType(jVal.Value, type);
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
      else {
        fieldName = StringUtil.ToCamelCase(prop.Name);
      }

      return fieldName;
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
