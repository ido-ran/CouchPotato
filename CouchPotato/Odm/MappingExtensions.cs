
namespace CouchPotato.Odm {
  public static class MappingExtensions {

    /// <summary>
    /// Get document type for entity instance.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static string DocTypeForEntity(this Mapping mapping, object entity) {
      return mapping.DocTypeForEntityType(entity.GetType());
    }
  }
}
