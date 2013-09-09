using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm.Internal {
  internal class EntityDefinitionBuilder {

    private readonly Type entityType;

    public EntityDefinitionBuilder(Type entityType) {
      this.entityType = entityType;
    }

    public EntityDefinition Build() {
      List<EntityPropertyDefinition> propDefs = CreatePropertyDefinitions();
      return new EntityDefinition(entityType, propDefs);
    }

    private List<EntityPropertyDefinition> CreatePropertyDefinitions() {
      var propDefs = new List<EntityPropertyDefinition>();

      foreach (PropertyInfo prop in GetPropertiesOf()) {
        EntityPropertyDefinition propDef;

        if (IsKeyField(prop)) {
          propDef = CreateKeyPropertyDefinition(prop);
        }
        else if (IsSimpleType(prop)) {
          propDef = CreateValueTypeDefinition(prop);
        }
        else if (IsArray(prop)) {
          propDef = CreateArrayProperty(prop);
        }
        else if (IsCollection(prop)) {
          propDef = CreateCollectionDefinition(prop);
        }
        else if (IsToOneReference(prop)) {
          propDef = CreateToOneReferenceDefinition(prop);
        }
        else {
          propDef = null;
          DebugWriteLine("The property {0}.{1} is of type {2} which is not supported yet",
              prop.DeclaringType.Name, prop.Name, prop.PropertyType.Name);
        }

        if (propDef != null) {
          propDefs.Add(propDef);
        }
      }
      return propDefs;
    }

    private EntityPropertyDefinition CreateKeyPropertyDefinition(PropertyInfo prop) {
      return new KeyEntityPropertyDefinition(prop);
    }

    private EntityPropertyDefinition CreateToOneReferenceDefinition(PropertyInfo prop) {
      return new ToOneEntityPropertyDefinition(prop);
    }

    private EntityPropertyDefinition CreateCollectionDefinition(PropertyInfo prop) {
      return new CollectionEntityPropertyDefinition(prop);
    }

    private EntityPropertyDefinition CreateArrayProperty(PropertyInfo prop) {
      return new ArrayEntityPropertyDefinition(prop);
    }

    private void DebugWriteLine(string p1, string p2, string p3, string p4) {
      throw new NotImplementedException();
    }

    private EntityPropertyDefinition CreateValueTypeDefinition(PropertyInfo prop) {
      return new ValueTypeEntityPropertyDefinition(prop);
    }

    private IEnumerable<PropertyInfo> GetPropertiesOf() {
      return entityType.GetProperties(
        BindingFlags.FlattenHierarchy |
        BindingFlags.Public |
        BindingFlags.Instance);
    }

    private static bool IsCollection(PropertyInfo prop) {
      return (typeof(ICollection<>).GUID.Equals(prop.PropertyType.GUID) ||
              typeof(ISet<>).GUID.Equals(prop.PropertyType.GUID));
    }

    private static bool IsArray(PropertyInfo prop) {
      return prop.PropertyType.IsArray;
    }

    private static bool IsSimpleType(PropertyInfo prop) {
      bool isSimple =
        prop.PropertyType.IsValueType ||
        typeof(string) == prop.PropertyType;

      return isSimple;
    }

    /// <summary>
    /// A property considered ToOne reference if it is a reference type
    /// and singular (not enumerable).
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    private static bool IsToOneReference(PropertyInfo prop) {
      bool isToOneRef =
        !prop.PropertyType.IsValueType &&
        !(typeof(IEnumerable).IsAssignableFrom(prop.PropertyType));

      return isToOneRef;
    }

    private static bool IsKeyField(PropertyInfo prop) {
      bool isKey = (prop.Name.Equals(Serializer.GetEntityIdPropertyName(prop.DeclaringType), StringComparison.OrdinalIgnoreCase));
      return isKey;
    }

    internal static string GetJsonFieldName(PropertyInfo prop) {
      string fieldName;
      if (IsKeyField(prop)) {
        fieldName = "_id";
      }
      else if (IsSimpleType(prop) || IsCollection(prop) || IsArray(prop)) {
        fieldName = StringUtil.ToCamelCase(prop.Name);
      }
      else if (IsToOneReference(prop)) {
        fieldName = StringUtil.ToCamelCase(prop.Name) + "ID";
      }
      else {
        throw new NotSupportedException("Not supported property info " + prop);
      }

      return fieldName;
    }
  }
}
