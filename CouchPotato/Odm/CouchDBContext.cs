using System;
using System.Collections.Generic;
namespace CouchPotato.Odm {
 public interface CouchDBContext {
    void Update(object entity);
    OdmView<T> View<T>(string viewName);
    void LoadRelated<T>(IEnumerable<T> sourceEntities, Action<LoadRelatedOptionsBuilder<T>> configOptions);
    void SaveChanges(bool allOrNothing = false);
    void Clear();
    ReduceView<T> ReduceView<T>(string viewName);
    string GetDocIDFromView(string viewName, params object[] key);
    DeleteByView DeleteByView(string viewName);
    void Delete(object entity);
 }
}
