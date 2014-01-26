using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouchPotato.Odm;

namespace CouchPotato.Test {
  internal static class ContextTestHelper {
    public static CouchDBContextImpl BuildContextForTest(CouchDBClientAdapterMock couchDBClientMock) {
      var context = new CouchDBContextImpl(couchDBClientMock);

      context.Mapping.MapDocTypeToEntity("user", typeof(UserModel));
      context.Mapping.MapDocTypeToEntity("tenant", typeof(TenantModel));
      context.Mapping.MapDocTypeToEntity("plan", typeof(PlanModel));

      return context;
    }
  }
}
