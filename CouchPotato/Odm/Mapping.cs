using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm {
  public class Mapping {

    private readonly Dictionary<string, Type> docTypeToEntityType;
    private readonly Dictionary<Type, string> entityTypeToDocType;

    public Mapping() {
      docTypeToEntityType = new Dictionary<string, Type>();
      entityTypeToDocType = new Dictionary<Type, string>();
    }

    public void MapDocTypeToEntity(string docType, Type entityType) {
      docTypeToEntityType.Add(docType, entityType);
      entityTypeToDocType.Add(entityType, docType);
    }

    public Type EntityTypeForDocType(string docType) {
      Type entityType;
      if (!docTypeToEntityType.TryGetValue(docType, out entityType)) {
        throw new Exception("Fail to find entity document type for " + docType);
      }
      return entityType;
    }

    public string DocTypeForEntityType(Type entityType) {
      string docType;
      if (!entityTypeToDocType.TryGetValue(entityType, out docType)) {
        throw new Exception("Fail to find entity type for " + entityType);
      }
      return docType;
    }

  }
}
