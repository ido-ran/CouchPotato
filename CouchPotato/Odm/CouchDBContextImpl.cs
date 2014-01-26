using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CouchPotato.Annotations;
using CouchPotato.CouchClientAdapter;
using CouchPotato.Odm.Internal;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  /// <summary>
  /// Entry point to CouchPotato API.
  /// </summary>
  public class CouchDBContextImpl : CouchDBContext {

    private readonly CouchDBClientAdapter clientAdapter;
    private readonly IdentityMap identityMap;
    private readonly Mapping mapping;
    private readonly DocumentManager docMgr;
    private readonly Serializer serializer;

    public CouchDBContextImpl(CouchDBClientAdapter clientAdapter) {
      this.clientAdapter = clientAdapter;
      mapping = new Mapping();
      identityMap = new IdentityMap(this);
      docMgr = new DocumentManager();
      serializer = new Serializer(this);
    }

    public OdmView<T> View<T>(string viewName) {
      return new OdmView<T>(this, viewName);
    }

    /// <summary>
    /// Get raw value from entity document.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public T GetDocumentFieldForEntity<T>(object entity, string fieldName) {
      string id = IdentityMap.GetIdByEntity(entity);
      JToken documentField = DocumentManager.DocInfo(id).Document[fieldName];
      T value = (T) Serializer.ResolveValue(documentField, typeof(T));
      return value;
    }

    public string GetDocIDFromView(string viewName, params object[] key) {
      var viewOptions = new CouchViewOptions { Key = key.ToList() };
      JToken[] rows = clientAdapter.GetViewRows(viewName, viewOptions);

      string docid;
      if (rows.Length > 1) {
        throw new Exception("More than one row found in view " + viewName);
      }
      else if (rows.Length == 0) {
        // No document found
        docid = null;
      }
      else {
        docid = rows[0].Value<string>(CouchDBFieldsConst.ResultRowId);
      }

      return docid;
    }

    public CouchDBClientAdapter ClientAdaper {
      get { return clientAdapter; }
    }

    public Mapping Mapping {
      get { return mapping; }
    }

    internal Serializer Serializer {
      get { return serializer; }
    }

    /// <summary>
    /// Mark an entity as updated.
    /// </summary>
    /// <param name="entity"></param>
    public void Update(object entity) {
      if (entity == null) throw new ArgumentNullException("entity");
      EnsureEntityTypeMapped(entity.GetType());

      string id = IdentityMap.GetIdByEntity(entity);
      if (null == id) {
        // New entity
        DocumentManager.MarkInserted(GetEntityInstanceId(entity));
        IdentityMap.AddNewEntity(entity);
      }
      else {
        // Update managed entity.
        DocumentManager.MarkUpdated(id);
        IdentityMap.UpdateEntity(id, entity);
      }
    }

    /// <summary>
    /// Ensure that this entity type has mapping in this context.
    /// </summary>
    /// <param name="entityType"></param>
    private void EnsureEntityTypeMapped(Type entityType) {
      // We search for the document type, if it is not mapped an exception
      // will be thrown.
      mapping.DocTypeForEntityType(entityType);
    }

    public void Delete(object entity) {
      string id = IdentityMap.GetIdByEntity(entity);
      if (null == id) {
        // Entity was not found in the identity map
        throw new Exception("Entity was not found");
      }
      else {
        // Exist entity, mark as deleted.
        DocumentManager.MarkDeleted(id);
      }
    }

    internal static string GetEntityInstanceId(object entity) {
      PropertyInfo idGetter = Serializer.GetEntityIdGetter(entity.GetType());
      return (string)idGetter.GetValue(entity);
    }

    /// <summary>
    /// Commit changes made in the context to the database.
    /// </summary>
    /// <param name="allOrNothing">Indicate if the bulk API should use all_or_nothing parameter.</param>
    public void SaveChanges(bool allOrNothing = false) {

      Tuple<CouchDocInfo, object>[] modifiedEntities = GetModifiedEntities();
      if (modifiedEntities.Length > 0) {
        BulkUpdater bulkUpdater = clientAdapter.CreateBulkUpdater(allOrNothing);

        AddEntitiesToBatchUpdater(modifiedEntities, bulkUpdater);
        BulkResponse response = bulkUpdater.Execute();
        ProcessBatchUpdateResult(response);
      }
    }

    private void ProcessBatchUpdateResult(BulkResponse response) {
      bool hasError = false;

      foreach (BulkResponseRow row in response.Rows) {
        if (row.HasError) {
          hasError = true;
          docMgr.MarkError(row.Id, row.Error);
        }
        else {
          docMgr.MarkClean(row.Id, row.Rev);
        }
      }

      if (hasError) {
        throw new SaveChangesException();
      }
    }

    private void AddEntitiesToBatchUpdater(Tuple<CouchDocInfo, object>[] modifiedEntities, BulkUpdater bulkUpdater) {
      foreach (Tuple<CouchDocInfo, object> entityWithInfo in modifiedEntities) {
        if (entityWithInfo.Item1.State == DocumentState.Delete) {
          bulkUpdater.Delete(entityWithInfo.Item1.Id, entityWithInfo.Item1.Rev);
        }
        else {
          JObject serializedDoc = IdentityMap.EntityAsDocument(entityWithInfo.Item1.Rev, entityWithInfo.Item2);
          bulkUpdater.Update(serializedDoc);
        }
      }
    }

    private Tuple<CouchDocInfo, object>[] GetModifiedEntities() {
      CouchDocInfo[] modifiedDocInfos = DocumentManager.GetModifiedDocuments();
      var modifiedEntities =
          from di in modifiedDocInfos
          select Tuple.Create(di, IdentityMap.GetEntityById(di.Id));

      return modifiedEntities.ToArray();
    }

    internal IdentityMap IdentityMap {
      get { return identityMap; }
    }

    internal DocumentManager DocumentManager {
      get { return docMgr; }
    }

    /// <summary>
    /// Process document rows.
    /// </summary>
    /// <param name="rows"></param>
    /// <returns></returns>
    internal EntitiesProcessResult Process(
      JToken[] rows, OdmViewProcessingOptions processingOptions) {

      EntitiesProcessResult result = IdentityMap.Process(rows, processingOptions);

      DocumentManager.Add(result.NewEntities);

      return result;
    }

    /// <summary>
    /// Load specified related entities of the sourceEntities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sourceEntities"></param>
    /// <param name="configOptions"></param>
    public void LoadRelated<T>(IEnumerable<T> sourceEntities, Action<LoadRelatedOptionsBuilder<T>> configOptions) {
      var optionsBuilder = new LoadRelatedOptionsBuilder<T>();
      configOptions(optionsBuilder);
      LoadRelatedOptions options = optionsBuilder.Build();

      // First pre-process all the source entities and find the relatd entities to load.
      LoadRelatedPreProcessInfo preProcessInfo = LoadRelatedPreProcess(sourceEntities, options);

      // Load the related entities and set the association properties.
      LoadRelatedPostProcess(sourceEntities, options, preProcessInfo);
    }

    private void LoadRelatedPostProcess<T>(
      IEnumerable<T> sourceEntities,
      LoadRelatedOptions options, LoadRelatedPreProcessInfo preProcessInfo) {

      JToken[] rows = ClientAdaper.GetDocuments(preProcessInfo.EntityIdsToLoad);
      EntitiesProcessResult processResult = Process(rows, new OdmViewProcessingOptions());

      Dictionary<string, EntitiesProcessResult> viewsSelectResult = SelectToManyFromViews(preProcessInfo);

      foreach (T sourceEntity in sourceEntities) {
        string entityId = GetEntityInstanceId(sourceEntity);
        JToken document = DocumentManager.DocInfo(entityId).Document;

        // ToOne
        foreach (PropertyInfo toOneProp in options.ToOne) {
          string relatedEntityId = GetRelatedToOneEntityId(document, toOneProp);
          object relatedToOneEntity = processResult.GetEntity(relatedEntityId);

          // This line was comment out because it is possible to get null related entity
          // when the property is optional. This check needs to be done only for required properties.
          //if (relatedToOneEntity == null) throw new Exception("Fail to find ToOne related entity for property " + toOneProp);
          toOneProp.SetValue(sourceEntity, relatedToOneEntity);
        }

        // ToOne already loaded
        foreach (PropertyInfo toOneProp in options.ToOneExist) {
          string relatedEntityId = GetRelatedToOneEntityId(document, toOneProp);
          object relatedToOneEntity = identityMap.GetEntityById(relatedEntityId);
          if (relatedToOneEntity == null) throw new Exception("Fail to find ToOneExist related entity for property " + toOneProp);
          toOneProp.SetValue(sourceEntity, relatedToOneEntity);
        }

        // ToMany Direct
        foreach (PropertyInfo toManyDirectProp in options.ToManyDirect) {
          string[] relatedEntitiesIds = GetRelatedToManyEntitiesIds(document, toManyDirectProp);
          if (relatedEntitiesIds != null) {
            serializer.SetDirectAssoicationCollectionProperty(sourceEntity, toManyDirectProp, relatedEntitiesIds);
          }
        }

        // ToMany Inverse
        foreach (LoadRelatedWithViewInfo toManyViewInfo in options.ToManyView) {
          EntitiesProcessResult viewProcessResult = viewsSelectResult[toManyViewInfo.ViewName];
          string[] relatedEntitiesIds = viewProcessResult.GetRelatedEntitiesIds(entityId);

          AssociationAttribute associationAttr = AssociationAttribute.GetSingle(toManyViewInfo.PropertyInfo);
          serializer.SetInverseAssociationCollectionInternal(
            sourceEntity, toManyViewInfo.PropertyInfo, associationAttr, relatedEntitiesIds);
        }

      }
    }

    private Dictionary<string, EntitiesProcessResult> SelectToManyFromViews(LoadRelatedPreProcessInfo preProcessInfo) {
      var viewSelectResult = new Dictionary<string, EntitiesProcessResult>();
      foreach (KeyValuePair<string, string[]> viewSelectionInfo in preProcessInfo.ViewsToSelect) {
        CouchViewOptions viewOptions = new CouchViewOptions
        {
          IncludeDocs = true,
          Keys = viewSelectionInfo.Value.Cast<object>().ToList()
        };

        JToken[] rows = clientAdapter.GetViewRows(viewSelectionInfo.Key, viewOptions);
        EntitiesProcessResult processResult = Process(rows, new OdmViewProcessingOptions());

        viewSelectResult.Add(viewSelectionInfo.Key, processResult);
      }

      return viewSelectResult;
    }

    private LoadRelatedPreProcessInfo LoadRelatedPreProcess<T>(IEnumerable<T> sourceEntities, LoadRelatedOptions options) {
      var builder = new LoadRelatedPreProcessInfoBuilder();

      foreach (T sourceEntity in sourceEntities) {
        string entityId = GetEntityInstanceId(sourceEntity);
        JToken document = DocumentManager.DocInfo(entityId).Document;

        Debug.Assert(document != null, "Fail to find the original document of the entity " + sourceEntity);

        // Find all ToOne associations to load
        foreach (PropertyInfo toOneProp in options.ToOne) {
          string relatedEntityId = GetRelatedToOneEntityId(document, toOneProp);
          if (!string.IsNullOrEmpty(relatedEntityId)) {
            builder.RelatedEntityIdsToLoad.Add(relatedEntityId);
          }
        }

        // Find all ToMany associations to load
        foreach (PropertyInfo toManyDirectProp in options.ToManyDirect) {
          string[] relatedEntitiesIds = GetRelatedToManyEntitiesIds(document, toManyDirectProp) ?? new string[0];
          foreach (string relatedEntityId in relatedEntitiesIds) {
            builder.RelatedEntityIdsToLoad.Add(relatedEntityId);
          }
        }

        // Prepare all Inverse-ToMany with views.
        foreach (LoadRelatedWithViewInfo inverseToMany in options.ToManyView) {
          builder.AddViewSelection(inverseToMany.ViewName, GetEntityInstanceId(sourceEntity));
        }
      }

      return builder.Build();
    }

    private string[] GetRelatedToManyEntitiesIds(JToken document, PropertyInfo toManyDirectProp) {
      return (string[])Serializer.GetJsonArray(toManyDirectProp, document, typeof(string));
    }

    private static string GetRelatedToOneEntityId(JToken document, PropertyInfo toOneProp) {
      string jsonFieldName = EntityDefinitionBuilder.GetJsonFieldName(toOneProp);
      string relatedEntityId = document.Value<string>(jsonFieldName);
      return relatedEntityId;
    }

    public DeleteByView DeleteByView(string viewName) {
      return new DeleteByView(this, viewName);
    }

    /// <summary>
    /// Clear the context from any loaded and changed entities.
    /// </summary>
    public void Clear() {
      IdentityMap.Clear();
      DocumentManager.Clear();
    }

    public ReduceView<T> ReduceView<T>(string viewName) {
      return new ReduceView<T>(this, viewName);
    }
  }
}
