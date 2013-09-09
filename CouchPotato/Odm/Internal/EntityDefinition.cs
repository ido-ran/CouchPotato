using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm.Internal {
  internal class EntityDefinition {

    private readonly Type entityType;
    private readonly EntityPropertyDefinition[] propDefs;

    public EntityDefinition(Type entityType, List<EntityPropertyDefinition> propDefs) {
      this.entityType = entityType;
      this.propDefs = propDefs.ToArray();
    }

    internal object CreateInstance() {
      return Activator.CreateInstance(entityType);
    }

    public IEnumerable<EntityPropertyDefinition> Properties {
      get { return propDefs; }
    }

    public override string ToString() {
      return string.Format("{0} {1}", GetType().Name, entityType.Name);
    }
  }
}
