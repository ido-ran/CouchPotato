using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json.Linq;
using CouchPotato.Annotations;
using CouchPotato.Odm.Internal;
using System.Globalization;

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

      EntityDefinition entityDef = context.Mapping.GetEntityDefinition(entityType);
      object proxy = entityDef.CreateInstance();
      FillProxy(entityDef, proxy, doc, id, preProcess, processingOptions, emptyProxy);
      return proxy;
    }

    public JObject Serialize(string rev, string docType, object entity) {
      JObject document = new JObject();

      document.Add(CouchDBFieldsConst.DocType, docType);

      if (rev != null) {
        document.Add(CouchDBFieldsConst.DocRev, rev);
      }

      EntityDefinition entityDef = context.Mapping.GetEntityDefinition(entity.GetType());
      FillDocument(entityDef, document, entity);

      return document;
    }

    private void FillDocument(EntityDefinition entityDef, JObject doc, object entity) {
      EntityPropertyDefinition[] docWritePropOrder = OrderPropertiesForDocumentWriting(entityDef.Properties);
      foreach (EntityPropertyDefinition propDef in docWritePropOrder) {
        propDef.Write(entity, doc);
      }
    }

    private EntityPropertyDefinition[] OrderPropertiesForDocumentWriting(
      IEnumerable<EntityPropertyDefinition> properties) {

      return properties
        .OrderBy(x => x, new EntityPropertiesDefinitionDocumentWritingComparer())
        .ToArray();
    }

    internal static bool IsNull(object value) {
      return (null == value);
    }

    internal void FillProxy(
      EntityDefinition entityDef,
      object proxy, JToken doc, string id, PreProcessInfo preProcess,
      OdmViewProcessingOptions processingOptions, bool emptyProxy) {

      foreach (EntityPropertyDefinition propDef in entityDef.Properties) {
        propDef.Read(proxy, doc, id, preProcess, processingOptions, emptyProxy, context);
      }

      //foreach (PropertyInfo prop in GetPropertiesOf(proxy)) {
      //  if (emptyProxy && IsSimpleType(prop)) {
      //    ReadValueType(proxy, prop, doc);
      //  }
      //  else if (emptyProxy && IsArray(prop)) {
      //    ReadArray(proxy, prop, doc);
      //  }
      //  else if (IsCollection(prop)) {
      //    CreateReferenceProxies(proxy, prop, doc, id, preProcess, processingOptions, emptyProxy);
      //  }
      //  else if (IsToOneReference(prop)) {
      //    DebugWriteLine("The property {0}.{1} is entity reference which is supported yet",
      //        prop.DeclaringType.Name, prop.Name, prop.PropertyType.Name);
      //  }
      //  else {
      //    DebugWriteLine("The property {0}.{1} is of type {2} which is not supported yet",
      //        prop.DeclaringType.Name, prop.Name, prop.PropertyType.Name);
      //  }
      //}
    }

    private void DebugWriteLine(string message, params object[] arguments) {
      Debug.WriteLine(string.Format(message, arguments));
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

    internal void SetDirectAssoicationCollectionProperty(object proxy, PropertyInfo prop, Array clrArr) {
      Type elementType = prop.PropertyType.GenericTypeArguments[0];
      Type associateCollectionClosedType = CreateAssociateCollectionType(prop.PropertyType, elementType);

      object associateCollection = Activator.CreateInstance(
        associateCollectionClosedType,
        proxy, (string[])clrArr, context, null);

      prop.SetValue(proxy, associateCollection);
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

    internal static Array GetJsonArray(PropertyInfo prop, JToken doc) {
      Type elementType = prop.PropertyType.GetElementType();
      return GetJsonArray(prop, doc, elementType);
    }

    internal static JArray GetJArray(PropertyInfo prop, JToken doc) {
      string jsonFieldName = EntityDefinitionBuilder.GetJsonFieldName(prop);
      var jArr = (JArray)doc[jsonFieldName];
      return jArr;
    }

    internal static Array ResolveArray(Type elementType, JArray jArr) {
      Array clrArr = Array.CreateInstance(elementType, jArr.Count);

      for (int index = 0; index < jArr.Count; index++) {
        object value = ResolveValue(jArr[index], elementType);
        clrArr.SetValue(value, index);
      }
      return clrArr;
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
          object intermidValue = ConvertValueTo(jVal.Value, nullableUnderlyingType ?? desiredType);
          if (nullableUnderlyingType != null) {
            convertedValue = Activator.CreateInstance(desiredType, intermidValue);
          }
          else {
            convertedValue = intermidValue;
          }
        }
        else {
          throw new NotImplementedException("Unable to handle Json toekn of type " + jToken.GetType().Name);
        }
      }

      return convertedValue;
    }

    private static object ConvertValueTo(object serializedValue, Type desiredType) {
      if (serializedValue == null) return null;

      if (typeof(DateTime) == desiredType) {
        return ConvertToDateTime(serializedValue);
      }
      else {
        return Convert.ChangeType(serializedValue, desiredType);
      }
    }

    private static object ConvertToDateTime(object serializedValue) {
      if (serializedValue.GetType() == typeof(string)) {
        return ConvertStringToDateTime((string)serializedValue);
      }
      else {
        return ConvertRawDateTimeToUtcDateTime((DateTime)serializedValue);
      }
    }

    internal static object ConvertRawDateTimeToUtcDateTime(DateTime rawDateTime) {
      return new DateTime(rawDateTime.Ticks, DateTimeKind.Utc);
    }

    private static object ConvertStringToDateTime(string serializedValue) {
      var dateTime = DateTime.ParseExact(serializedValue, "O",
        CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      return dateTime;
    }

    internal static string GetEntityIdPropertyName(Type entityType) {
      return entityType.Name + "ID";
    }

    internal static PropertyInfo GetEntityIdGetter(Type entityType) {
      PropertyInfo idGetter = entityType.GetProperty(Serializer.GetEntityIdPropertyName(entityType));

      if (idGetter == null) {
        throw new Exception("Fail to find ID getter property for " + entityType);
      }

      return idGetter;
    }


    internal void ReFillProxy(
      object entity,
      JToken doc,
      string id,
      PreProcessInfo preProcess,
      OdmViewProcessingOptions processingOptions) {

      EntityDefinition entityDef = context.Mapping.GetEntityDefinition(entity.GetType());
      FillProxy(entityDef, entity, doc, id, preProcess, processingOptions, false);
    }

    internal static object ConvertJValueToClrValue(JToken jValue) {
      object clrValue = null;

      switch (jValue.Type) {
        case JTokenType.Array:
          break;
        case JTokenType.Boolean:
          break;
        case JTokenType.Bytes:
          break;
        case JTokenType.Comment:
          break;
        case JTokenType.Constructor:
          break;
        case JTokenType.Date:
          clrValue = ConvertRawDateTimeToUtcDateTime(jValue.Value<DateTime>());
          break;
        case JTokenType.Float:
          break;
        case JTokenType.Guid:
          break;
        case JTokenType.Integer:
          clrValue = jValue.Value<int>();
          break;
        case JTokenType.None:
          break;
        case JTokenType.Null:
          break;
        case JTokenType.Object:
          break;
        case JTokenType.Property:
          break;
        case JTokenType.Raw:
          break;
        case JTokenType.String:
          clrValue = jValue.Value<string>();
          break;
        case JTokenType.TimeSpan:
          break;
        case JTokenType.Undefined:
          break;
        case JTokenType.Uri:
          break;
        default:
          break;
      }

      if (clrValue == null) throw new Exception("Fail to convert JValue to CLR. Type " + jValue);
      return clrValue;
    }
  }
}
