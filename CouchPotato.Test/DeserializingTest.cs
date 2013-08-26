using System;
using System.Collections.Generic;
using CouchPotato.Odm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Test {
  [TestClass]
  public class DeserializingTest {

    private class SimpleEntity {
      public int SimpleEntityId { get; set; }
      public int Age { get; set; }
      public string Name { get; set; }
    }

    private class PersonWithFriends {
      public int PersonWithFriendsId { get; set; }
      public int Age { get; set; }
      public string Name { get; set; }
      public ICollection<PersonWithFriends> Friends { get; set; }
    }

    [TestMethod]
    public void Entity_With_Simple_Properties() {
      Serializer subject = new Serializer(null);
      string doc =
@"{
    _id: ""1"",
    _rev: ""1-1edc9b67751f21e58895635c4eb47456"",
    age: 22,
    name: ""lulu""
  }";
      JObject json = JObject.Parse(doc);
      var actualEntity = (SimpleEntity)subject.CreateProxy(json, typeof(SimpleEntity), "1", null, null, true);

      Assert.IsNotNull(actualEntity, "Fail to deserialize simple entity");
      Assert.AreEqual(22, actualEntity.Age);
      Assert.AreEqual("lulu", actualEntity.Name);
    }

    [TestMethod]
    public void Entity_With_Associated_Collection() {
      Serializer subject = new Serializer(null);
      string doc =
@"{
    _id: ""1"",
    _rev: ""1-1edc9b67751f21e58895635c4eb47456"",
    age: 22,
    name: ""lulu"",
    friends: [""2"", ""3""]
  }";
      JObject json = JObject.Parse(doc);
      var actualEntity = (PersonWithFriends)subject.CreateProxy(json, typeof(PersonWithFriends), "1", null, null, true);

      Assert.IsNotNull(actualEntity, "Fail to deserialize simple entity");
      Assert.AreEqual(22, actualEntity.Age);
      Assert.AreEqual("lulu", actualEntity.Name);
      Assert.IsInstanceOfType(actualEntity.Friends, typeof(AssociationCollection<PersonWithFriends>));
    }
  }
}
