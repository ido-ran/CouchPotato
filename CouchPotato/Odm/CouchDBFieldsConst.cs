
namespace CouchPotato.Odm {
  /// <summary>
  /// Constant names for CouchDB json fields.
  /// </summary>
  internal static class CouchDBFieldsConst {
    /// <summary>
    /// Field we add to each document that represent an entity to indicate the entity type.
    /// </summary>
    public const string DocType = "$type";

    /// <summary>
    /// CouchDB has this field in each document.
    /// </summary>
    public const string DocId = "_id";

    /// <summary>
    /// CouchDB has this field in each document.
    /// </summary>
    public const string DocRev = "_rev";

    /// <summary>
    /// CouchDB has this field on documents that were deleted.
    /// </summary>
    public const string DocDelete = "_deleted";

    /// <summary>
    /// CouchDB use this field in result of view to indicate the key emmited.
    /// </summary>
    public const string Key = "key";

    /// <summary>
    /// CouchDB use this field in result of view to indicate the value emmited.
    /// </summary>
    public const string ViewValue = "value";

    /// <summary>
    /// CouchDB use this field in result of view when include_docs=true paramter is used.
    /// </summary>
    public const string ResultRowDoc = "doc";

    /// <summary>
    /// CouchDB use this field in result of view to indicate the document id used to generate the row.
    /// </summary>
    public const string ResultRowId = "id";
  }
}
