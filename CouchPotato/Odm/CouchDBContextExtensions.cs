using System;

namespace CouchPotato.Odm {
  /// <summary>
  /// Extension methods for CouchDBContext.
  /// </summary>
  public static class CouchDBContextExtensions {
    /// <summary>
    /// Load related entities of single source entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="sourceEntity"></param>
    /// <param name="configOptions"></param>
    public static void LoadRelated<T>(this CouchDBContext context, T sourceEntity, Action<LoadRelatedOptionsBuilder<T>> configOptions) {
      T[] arrayOfOne = new T[] { sourceEntity };
      context.LoadRelated(arrayOfOne, configOptions);
    }
  }
}
