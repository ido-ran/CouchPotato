using System;
using System.Linq;
using System.Collections.Generic;
using CouchPotato.Odm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Test {
  [TestClass]
  public class SerializingTest {

    private class SimpleEntity {
      public int SimpleEntityID { get; set; }
      public int Age { get; set; }
      public string Name { get; set; }
    }

    private class PersonWithFriends {
      public int PersonWithFriendsID { get; set; }
      public int Age { get; set; }
      public string Name { get; set; }
      public ICollection<PersonWithFriends> Friends { get; set; }
    }

    private class EntityWithArray {
      public string EntityWithArrayID { get; set; }
      public string[] StringArray { get; set; }
      public int[] IntArray { get; set; }
    }

    [TestMethod]
    public void SerializeRevision() {
      var entity = new SimpleEntity
      {
        SimpleEntityID = 49,
        Age = 12,
        Name = "Sara"
      };

      Serializer subject = new Serializer(null);
      JObject doc = subject.Serialize("2-update", "simple-entity", entity);

      Assert.AreEqual("2-update", doc.Value<string>("_rev"));
    }

    [TestMethod]
    public void SerializeDocumentType() {
      var entity = new SimpleEntity
      {
        SimpleEntityID = 49,
        Age = 12,
        Name = "Sara"
      };

      Serializer subject = new Serializer(null);
      JObject doc = subject.Serialize("2-update", "simple-entity", entity);

      Assert.AreEqual("simple-entity", doc.Value<string>("$type"));
    }

    [TestMethod]
    public void Entity_With_Simple_Properties() {
      var entity = new SimpleEntity
      {
        SimpleEntityID = 49,
        Age = 12,
        Name = "Sara"
      };

      Serializer subject = new Serializer(null);
      JObject doc = subject.Serialize("2-update", "simple-entity", entity);
      
      Assert.IsNotNull(doc, "Fail to serialize simple entity");
      Assert.AreEqual(49, doc.Value<int>("_id"));
      Assert.AreEqual(12, doc.Value<int>("age"));
      Assert.AreEqual("Sara", doc.Value<string>("name"));
    }

    [TestMethod]
    public void Entity_With_Array() {
      var entity = new EntityWithArray
      {
        EntityWithArrayID = "hello",
        StringArray = new [] { "A", "BB", "CCC" },
        IntArray = new [] { 80, 79, 78 }
      };

      Serializer subject = new Serializer(null);
      JObject doc = subject.Serialize("2-update", "entity-with-array", entity);

      Assert.IsNotNull(doc, "Fail to serialize simple entity");
      CollectionAssert.AreEqual(new[] { "A", "BB", "CCC" }, JArrayHelper.ArrayOf<string>((JArray)doc["stringArray"]));
      CollectionAssert.AreEqual(new[] { 80, 79, 78 }, JArrayHelper.ArrayOf<int>((JArray)doc["intArray"]));
    }

    [TestMethod]
    public void Entity_With_Empty_Associated_Collection() {
      var entity = new PersonWithFriends
      {
        PersonWithFriendsID = 10,
        Age = 93,
        Name = "Yoni",
        Friends = new List<PersonWithFriends>()
      };
      
      Serializer subject = new Serializer(null);
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      Assert.IsNotNull(doc, "Fail to serialize simple entity");
      Assert.AreEqual(10, doc.Value<int>("_id"));
      Assert.AreEqual(93, doc.Value<int>("age"));
      Assert.AreEqual("Yoni", doc.Value<string>("name"));
      Assert.IsNull(doc["friends"]);
    }

    [TestMethod]
    public void Entity_With_SingleEntry_Associated_Collection() {
      var entity = new PersonWithFriends
      {
        PersonWithFriendsID = 10,
        Age = 93,
        Name = "Yoni",
        Friends = new List<PersonWithFriends>(new [] {
         new PersonWithFriends {
            PersonWithFriendsID = 99
         }
        })
      };

      Serializer subject = new Serializer(null);
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      Assert.IsNotNull(doc, "Fail to serialize simple entity");
      Assert.AreEqual(10, doc.Value<int>("_id"));
      Assert.AreEqual(93, doc.Value<int>("age"));
      Assert.AreEqual("Yoni", doc.Value<string>("name"));

      var associatedArray = (JArray)doc["friends"];
      CollectionAssert.AreEqual(new[] { 99 }, JArrayHelper.ArrayOf<int>(associatedArray));
    }

    [TestMethod]
    public void Entity_With_MultipleEntry_Associated_Collection() {
      var entity = new PersonWithFriends
      {
        PersonWithFriendsID = 10,
        Age = 93,
        Name = "Yoni",
        Friends = new List<PersonWithFriends>(new[] {
         new PersonWithFriends {
            PersonWithFriendsID = 99
         },
         new PersonWithFriends {
            PersonWithFriendsID = 100
         },new PersonWithFriends {
            PersonWithFriendsID = 101
         }
        })
      };

      Serializer subject = new Serializer(null);
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      Assert.IsNotNull(doc, "Fail to serialize simple entity");
      Assert.AreEqual(10, doc.Value<int>("_id"));
      Assert.AreEqual(93, doc.Value<int>("age"));
      Assert.AreEqual("Yoni", doc.Value<string>("name"));

      var associatedArray = (JArray)doc["friends"];
      CollectionAssert.AreEqual(new[] { 99, 100, 101 }, JArrayHelper.ArrayOf<int>(associatedArray));
    }

    private static class JArrayHelper {

      public static T[] ArrayOf<T>(JArray jArray) {
        return jArray.Select(jt => jt.Value<T>()).ToArray();
      }
    }
  }
}
