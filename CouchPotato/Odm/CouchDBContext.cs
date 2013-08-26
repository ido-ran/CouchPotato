using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CouchPotato.Annotations;
using CouchPotato.CouchClientAdapter;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  /// <summary>
  /// Entry point to CouchPotato API.
  /// </summary>
  public class CouchDBContext {

    private readonly CouchDBClientAdapter clientAdapter;
    private readonly IdentityMap identityMap;
    private readonly Mapping mapping;
    private readonly DocumentManager docMgr;
    private readonly Serializer serializer;

    public CouchDBContext(CouchDBClientAdapter clientAdapter) {
      this.clientAdapter = clientAdapter;
      mapping = new Mapping();
      identityMap = new IdentityMap(this);
      docMgr = new DocumentManager();
      serializer = new Serializer(this);
    }

    public OdmView<T> View<T>(string viewName) {
      return new OdmView<T>(this, viewName);
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
      string id = IdentityMap.GetIdByEntity(entity);
      if (null == id) {
        // New entity
        DocumentManager.MarkInserted(GetEntityInstanceId(entity));
        IdentityMap.AddNewEntity(entity);
      }
      else {
        // Update managed entity.
        DocumentManager.MarkUpdated(id);
      }
    }

    internal string GetEntityInstanceId(object entity) {
      PropertyInfo idGetter = Serializer.GetEntityIdGetter(entity.GetType());
      return (string)idGetter.GetValue(entity);
    }

    /// <summary>
    /// Commit changes made in the context to the database.
    /// </summary>
    public void SaveChanges() {

      Tuple<CouchDocInfo, object>[] modifiedEntities = GetModifiedEntities();
      if (modifiedEntities.Length > 0) {
        BulkUpdater bulkUpdater = clientAdapter.CreateBulkUpdater();

        foreach (Tuple<CouchDocInfo, object> entityWithInfo in modifiedEntities) {
          JObject serializedDoc = IdentityMap.EntityAsDocument(entityWithInfo.Item1.Rev, entityWithInfo.Item2);
          bulkUpdater.Update(serializedDoc);
        }

        BulkResponse response = bulkUpdater.Execute();

        foreach (BulkResponseRow row in response.Rows) {
          docMgr.MarkClean(row.Id, row.Rev);
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
          Debug.Assert(relatedToOneEntity != null, "Fail to find ToOne related entity for property " + toOneProp);
          toOneProp.SetValue(sourceEntity, relatedToOneEntity);
        }

        // ToOne already loaded
        foreach (PropertyInfo toOneProp in options.ToOneExist) {
          string relatedEntityId = GetRelatedToOneEntityId(document, toOneProp);
          object relatedToOneEntity = identityMap.GetEntityById(relatedEntityId);
          Debug.Assert(relatedToOneEntity != null, "Fail to find ToOneExist related entity for property " + toOneProp);
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
      string jsonFieldName = Serializer.GetJsonFieldName(toOneProp);
      string relatedEntityId = document.Value<string>(jsonFieldName);
      return relatedEntityId;
    }
  }
}
