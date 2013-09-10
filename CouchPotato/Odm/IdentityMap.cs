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
    
    public IdentityMap(CouchDBContext context) {
      idToEntity = new Dictionary<string, object>();
      entityToId = new Dictionary<object, string>();
      this.context = context;
    }

    /// <summary>
    /// Process the docs and convert them to entities.
    /// </summary>
    /// <param name="rows"></param>
    /// <returns></returns>
    internal EntitiesProcessResult Process(
      JToken[] rows, OdmViewProcessingOptions processingOptions) {

      PreProcessInfo preProcess = PreProcess(rows);
      return PostProcess(preProcess, processingOptions);
    }

    private EntitiesProcessResult PostProcess(
      PreProcessInfo preProcess, OdmViewProcessingOptions processingOptions) {

      var resultBuilder = new EntitiesProcessResultBuilder();
      foreach (PreProcessEntityInfo preProcessRow in preProcess.Rows) {
        JToken row = preProcessRow.Row;
        JToken doc = row[CouchDBFieldsConst.ResultRowDoc];
        if (doc == null) throw new Exception("doc field was not found");

        string id = preProcessRow.Id;
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
          context.Serializer.ReFillProxy(entity, doc, id, preProcess, processingOptions);

          // Reuse exist entity.
          resultBuilder.AddExist(entity, idrev, doc, preProcessRow.Key);
        }
        else {
          // Found new entity
          entity = Deserialize(doc, preProcess, processingOptions);

          idToEntity.Add(id, entity);
          entityToId.Add(entity, id);

          resultBuilder.AddNew(entity, idrev, doc, preProcessRow.Key);
        }
      }

      return resultBuilder.BuildResult();
    }

    /// <summary>
    /// Add new entity to the identity map.
    /// </summary>
    /// <param name="entity"></param>
    public void AddNewEntity(object entity) {
      string id = CouchDBContext.GetEntityInstanceId(entity);
      idToEntity.Add(id, entity);
      entityToId.Add(entity, id);
    }

    private PreProcessInfo PreProcess(JToken[] rows) {
      var preProcessRows = new List<PreProcessEntityInfo>();

      foreach (JToken row in rows) {
        JToken doc = row[CouchDBFieldsConst.ResultRowDoc];
        if (doc == null) throw new Exception("doc field was not found");

        string id = doc.Value<string>(CouchDBFieldsConst.DocId);
        Type entityType = GetDocEntityType(doc);
        CouchDBViewRowKey key = ExtractRowKey(row);

        preProcessRows.Add(new PreProcessEntityInfo(id, entityType, key, row));
      }

      return new PreProcessInfo(preProcessRows.ToArray());
    }

    private CouchDBViewRowKey ExtractRowKey(JToken row) {
      object key;

      JToken token = row[CouchDBFieldsConst.Key];
      if (token.Type == JTokenType.Array) {
        var jArrayKey = (JArray)token;
        key = jArrayKey.Select(x => Serializer.ConvertJValueToClrValue(x)).ToArray();
      }
      else if (token.Type == JTokenType.Null) {
        key = null;
      }
      else {
        key = Serializer.ConvertJValueToClrValue(token);
      }

      return new CouchDBViewRowKey(key);
    }

    private object Deserialize(
      JToken doc, PreProcessInfo preProcess, OdmViewProcessingOptions processingOptions) {

      Type entityType = GetDocEntityType(doc);
      string entityId = GetDocId(doc);

      object entity = context.Serializer.CreateProxy(doc, entityType, entityId, preProcess, processingOptions, emptyProxy: true);
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
      return context.Serializer.Serialize(rev, docType, entity);
    }

    internal void Clear() {
      entityToId.Clear();
      idToEntity.Clear();
    }
  }
}
