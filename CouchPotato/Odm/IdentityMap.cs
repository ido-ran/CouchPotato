using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  internal class IdentityMap {

    /// <summary>
    /// The identity map of the loaded entities.
    /// </summary>
    private readonly Dictionary<string, object> idToEntity;
    private readonly Dictionary<object, string> entityToId;
    private readonly CouchDBContext context;
    private readonly Serializer serializer;

    public IdentityMap(CouchDBContext context) {
      idToEntity = new Dictionary<string, object>();
      entityToId = new Dictionary<object, string>();
      this.context = context;
      serializer = new Serializer(context);
    }

    /// <summary>
    /// Process the docs and convert them to entities.
    /// </summary>
    /// <param name="rows"></param>
    /// <returns></returns>
    internal EntitiesProcessResult Process(JToken[] rows) {
      PreProcessInfo preProcess = PreProcess(rows);
      return PostProcess(rows, preProcess);
    }

    private EntitiesProcessResult PostProcess(JToken[] rows, PreProcessInfo preProcess) {
      var resultBuilder = new EntitiesProcessResultBuilder();
      foreach (JToken row in rows) {
        JToken doc = row[CouchDBFieldsConst.ResultRowDoc];
        if (doc == null) throw new Exception("doc field was not found");

        string id = doc.Value<string>(CouchDBFieldsConst.DocId);
        string rev = doc.Value<string>(CouchDBFieldsConst.DocRev);
        var idrev = new IdRev(id, rev);

        object entity;
        if (idToEntity.TryGetValue(id, out entity)) {
          // PATCH: This line reload exist entities and should be remove in the future.
          // The reason it is here is because when loading main entity with associated entity
          // with many-to-many relations between them the related entity's assoication will
          // be partially loaded and when updating the association of the related entity
          // only part of the data will be avilable. This patch solve the problem
          // but it will introduce inconsistensis and also cost in performance.
          serializer.FillProxy(entity, doc, id, preProcess);

          // Reuse exist entity.
          resultBuilder.AddExist(entity, idrev);
        }
        else {
          // Found new entity
          entity = Deserialize(doc, preProcess);
          idToEntity.Add(id, entity);
          entityToId.Add(entity, id);

          resultBuilder.AddNew(entity, idrev);
        }
      }

      return resultBuilder.BuildResult();
    }

    private PreProcessInfo PreProcess(JToken[] rows) {
      var preProcessRows = new List<PreProcessEntityInfo>();

      foreach (JToken row in rows) {
        JToken doc = row[CouchDBFieldsConst.ResultRowDoc];
        if (doc == null) throw new Exception("doc field was not found");

        string id = doc.Value<string>(CouchDBFieldsConst.DocId);
        Type entityType = GetDocEntityType(doc);
        CouchDBViewRowKey key = ExtractRowKey(row);

        preProcessRows.Add(new PreProcessEntityInfo(id, entityType, key));
      }

      return new PreProcessInfo(preProcessRows.ToArray());
    }

    private CouchDBViewRowKey ExtractRowKey(JToken row) {
      object key;

      JToken token = row[CouchDBFieldsConst.Key];
      if (token.Type == JTokenType.Array) {
        var jArrayKey = (JArray)token;
        key = jArrayKey.Select(x => ConvertJValueToClrValue(x)).ToArray();
      }
      else if (token.Type == JTokenType.Null) {
        key = null;
      }
      else {
        key = ConvertJValueToClrValue(token);
      }

      return new CouchDBViewRowKey(key);
    }

    private object ConvertJValueToClrValue(JToken jValue) {
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

    private object Deserialize(JToken doc, PreProcessInfo preProcess) {
      Type entityType = GetDocEntityType(doc);
      string entityId = GetDocId(doc);

      object entity = serializer.CreateProxy(doc, entityType, entityId, preProcess);
      return entity;
    }

    private Type GetDocEntityType(JToken doc) {
      string docType = doc.Value<string>(CouchDBFieldsConst.DocType);
      Type entityType = context.Mapping.EntityTypeForDocType(docType);
      return entityType;
    }

    private string GetDocId(JToken doc) {
      string id = doc.Value<string>(CouchDBFieldsConst.DocId);
      return id;
    }

    /// <summary>
    /// Get entities from the specified type and keys.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    internal IEnumerable<T> GetEntities<T>(Type type, string[] keys) {
      var foundEntities = new List<T>(keys.Length);
      foreach (string key in keys) {
        object entity;
        if (!idToEntity.TryGetValue(key, out entity)) {
          throw new Exception("Fail to find entity with id " + key);
        }
        foundEntities.Add((T)entity);
      }

      return foundEntities;
    }

    internal string GetIdByEntity(object entity) {
      string id;
      entityToId.TryGetValue(entity, out id);
      return id;
    }

    internal object GetEntityById(string id) {
      return idToEntity[id];
    }

    internal JObject EntityAsDocument(string rev, object entity) {
      string docType = context.Mapping.DocTypeForEntity(entity);
      return serializer.Serialize(rev, docType, entity);
    }
  }
}
