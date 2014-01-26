using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouchPotato.Odm;

namespace CouchPotato.Test {
  internal static class SerializationTestHelper {

    internal static Serializer CreateSerializer(Type entityTypeToSerialize) {
      CouchDBContextImpl context = new CouchDBContextImpl(null);
      context.Mapping.MapDocTypeToEntity("e", entityTypeToSerialize);
      Serializer subject = context.Serializer;
      return subject;
    }


  }
}
