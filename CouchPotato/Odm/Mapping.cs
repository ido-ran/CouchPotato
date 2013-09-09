using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouchPotato.Odm.Internal;

namespace CouchPotato.Odm {
  public class Mapping {

    private readonly Dictionary<string, Type> docTypeToEntityType;
    private readonly Dictionary<Type, string> entityTypeToDocType;
    private readonly Dictionary<Type, EntityDefinition> entityDefinitions;

    public Mapping() {
      docTypeToEntityType = new Dictionary<string, Type>();
      entityTypeToDocType = new Dictionary<Type, string>();
      entityDefinitions = new Dictionary<Type, EntityDefinition>();
    }

    public void MapDocTypeToEntity(string docType, Type entityType) {
      docTypeToEntityType.Add(docType, entityType);
      entityTypeToDocType.Add(entityType, docType);

      var entityDefBuilder = new EntityDefinitionBuilder(entityType);
      EntityDefinition entityDef = entityDefBuilder.Build();
      entityDefinitions.Add(entityType, entityDef);
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

    internal EntityDefinition GetEntityDefinition(Type entityType) {
      EntityDefinition def;
      if (!entityDefinitions.TryGetValue(entityType, out def)) {
        throw new ArgumentException("No mapping was found for " + entityType);
      }
      return def;
    }
  }
}
