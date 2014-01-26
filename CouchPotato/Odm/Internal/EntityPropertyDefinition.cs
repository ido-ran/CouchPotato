using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm.Internal {
  internal interface EntityPropertyDefinition {

    /// <summary>
    /// Get the JSON field name of this property.
    /// </summary>
    string JsonFieldName { get; }

    /// <summary>
    /// Read value from the document to the entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="doc"></param>
    /// <param name="id"></param>
    /// <param name="preProcess"></param>
    /// <param name="processingOptions"></param>
    /// <param name="emptyProxy"></param>
    void Read(object entity, JToken doc, string id, 
      PreProcessInfo preProcess, OdmViewProcessingOptions processingOptions, bool emptyProxy,
      CouchDBContextImpl context);

    /// <summary>
    /// Write value from the entity to the document.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="doc"></param>
    void Write(object entity, JObject doc);
  }
}
