using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Odm.Internal {
  internal class KeyEntityPropertyDefinition : ValueTypeEntityPropertyDefinition {

    public KeyEntityPropertyDefinition(PropertyInfo propInfo)
      : base(propInfo, CouchDBFieldsConst.DocId) {
    }
  }
}
