using System;
using System.Linq;
using System.Collections.Generic;
using CouchPotato.Odm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using CouchPotato.Annotations;

namespace CouchPotato.Test {
  [TestClass]
  public class SerializerTest {

    private class SimpleEntity {
      public string SimpleEntityID { get; set; }
      public int Age { get; set; }
      public string Name { get; set; }
    }

    private class EntityWithToOneAssociation {
      public string EntityWithToOneAssociationID { get; set; }
      public SimpleEntity AnotherEntityRef { get; set; }
    }

    private class EntityWithToOneAssociationWithIDField {
      public string EntityWithToOneAssociationWithIDFieldID { get; set; }
      public SimpleEntity AnotherEntityRef { get; set; }
      public string AnotherEntityRefID { get; set; }
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

    private class EntityWithManyToManyInverseCollection {
      public string EntityWithManyToManyInverseCollectionID { get; set; }

      [Association("OtherProperty")]
      public ICollection<SimpleEntity> OtherEntities { get; set; }
    }

    private class EntityWithNullable {
      public string EntityWithNullableID { get; set; }
      public Nullable<DateTime> FinishedAt { get; set; }
      public string Name { get; set; }
      public int Age { get; set; }
    }

    private class Address {
      public string Street { get; set; }
      public int Number { get; set; }
    }

    private class EntityWithEmbeddedAddress {
      public string PeronName { get; set; }

      [Embedded]
      public Address Address { get; set; }
    }

    [TestMethod]
    public void SerializeRevision() {
      var entity = new SimpleEntity
      {
        SimpleEntityID = "49",
        Age = 12,
        Name = "Sara"
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(SimpleEntity));
      JObject doc = subject.Serialize("2-update", "simple-entity", entity);

      Assert.AreEqual("2-update", doc.Value<string>("_rev"));
    }

    [TestMethod]
    public void SerializeDocumentType() {
      var entity = new SimpleEntity
      {
        SimpleEntityID = "49",
        Age = 12,
        Name = "Sara"
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(SimpleEntity));
      JObject doc = subject.Serialize("2-update", "simple-entity", entity);

      Assert.AreEqual("simple-entity", doc.Value<string>("$type"));
    }

    [TestMethod]
    public void Entity_With_Simple_Properties() {
      var entity = new SimpleEntity
      {
        SimpleEntityID = "49",
        Age = 12,
        Name = "Sara"
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(SimpleEntity));
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
        StringArray = new[] { "A", "BB", "CCC" },
        IntArray = new[] { 80, 79, 78 }
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithArray));
      JObject doc = subject.Serialize("2-update", "entity-with-array", entity);

      Assert.IsNotNull(doc, "Fail to serialize simple entity");
      CollectionAssert.AreEqual(new[] { "A", "BB", "CCC" }, JArrayHelper.ArrayOf<string>((JArray)doc["stringArray"]));
      CollectionAssert.AreEqual(new[] { 80, 79, 78 }, JArrayHelper.ArrayOf<int>((JArray)doc["intArray"]));
    }

    [TestMethod]
    public void NullArrayOfReferenceTypeIsNotSerialize() {
      var entity = new EntityWithArray
      {
        EntityWithArrayID = "hello",
        StringArray = null
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithArray));
      JObject doc = subject.Serialize("2-update", "entity-with-array", entity);

      Assert.IsFalse(doc.Children().OfType<JProperty>().Any(x => x.Name.Equals("stringArray")));
    }

    [TestMethod]
    public void NullArrayOfValueTypeIsNotSerialize() {
      var entity = new EntityWithArray
      {
        EntityWithArrayID = "hello",
        IntArray = null
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithArray));
      JObject doc = subject.Serialize("2-update", "entity-with-array", entity);

      Assert.IsFalse(doc.Children().OfType<JProperty>().Any(x => x.Name.Equals("intArray")));
    }

    [TestMethod]
    public void DeserializeOfNullReferenceTypeArray_PropertyIsNull() {

      string serializedEntity = "{ EntityWithArrayID: \"e1\" }";
      JObject jobj = JObject.Parse(serializedEntity);

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithArray));
      var entity = (EntityWithArray)subject.CreateProxy(jobj, typeof(EntityWithArray), "e1", null, null, true);

      Assert.IsNull(entity.StringArray);
    }

    [TestMethod]
    public void DeserializeOfEmptyReferenceTypeArray_PropertyIsEmptyArray() {

      string serializedEntity = "{ EntityWithArrayID: \"e1\", stringArray: [] }";
      JObject jobj = JObject.Parse(serializedEntity);

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithArray));
      var entity = (EntityWithArray)subject.CreateProxy(jobj, typeof(EntityWithArray), "e1", null, null, true);

      Assert.IsNotNull(entity.StringArray);
      Assert.AreEqual(0, entity.StringArray.Length);
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

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(PersonWithFriends));
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
        Friends = new List<PersonWithFriends>(new[] {
         new PersonWithFriends {
            PersonWithFriendsID = 99
         }
        })
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(PersonWithFriends));
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

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(PersonWithFriends));
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      Assert.IsNotNull(doc, "Fail to serialize simple entity");
      Assert.AreEqual(10, doc.Value<int>("_id"));
      Assert.AreEqual(93, doc.Value<int>("age"));
      Assert.AreEqual("Yoni", doc.Value<string>("name"));

      var associatedArray = (JArray)doc["friends"];
      CollectionAssert.AreEqual(new[] { 99, 100, 101 }, JArrayHelper.ArrayOf<int>(associatedArray));
    }

    [TestMethod]
    public void NotSerializingInverseCollection() {
      var entity = new EntityWithManyToManyInverseCollection
      {
        EntityWithManyToManyInverseCollectionID = "49",
        OtherEntities = new List<SimpleEntity>(new[]
        {
          new SimpleEntity(),
          new SimpleEntity(),
          new SimpleEntity()
        })
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithManyToManyInverseCollection));
      JObject doc = subject.Serialize("2-update", "entity-with-inverse-collection", entity);

      Assert.IsNull(doc.Value<object>("otherEntities"));
    }

    [TestMethod]
    public void ResolveValue_DateTime_To_Nullable_DateTime() {
      JToken token = JToken.FromObject(new DateTime(2001, 02, 24, 11, 10, 00, DateTimeKind.Unspecified));
      Type desiredType = typeof(Nullable<DateTime>);

      object actual = Serializer.ResolveValue(token, desiredType);

      Assert.IsInstanceOfType(actual, desiredType);

      var nullableActual = (Nullable<DateTime>)actual;
      DateTime expected = new DateTime(2001, 02, 24, 11, 10, 00, DateTimeKind.Utc);
      Assert.AreEqual(expected, nullableActual.Value);
    }

    [TestMethod]
    public void ResolveValue_DateTime_To_DateTime() {
      JToken token = JToken.FromObject(new DateTime(2001, 02, 24, 11, 10, 00, DateTimeKind.Unspecified));
      Type desiredType = typeof(DateTime);

      object actual = Serializer.ResolveValue(token, desiredType);
      Assert.IsInstanceOfType(actual, desiredType);

      DateTime expected = new DateTime(2001, 02, 24, 11, 10, 00, DateTimeKind.Utc);
      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ResolveValue_Int_To_String() {
      JToken token = JToken.FromObject(34);
      Type desiredType = typeof(string);

      object actual = Serializer.ResolveValue(token, desiredType);
      Assert.IsInstanceOfType(actual, desiredType);
    }

    [TestMethod]
    public void NullNullableValueIsNotSerialized() {
      var entity = new EntityWithNullable
      {
        EntityWithNullableID = "e1",
        FinishedAt = null,
        Age = 10
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithNullable));
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      Assert.IsFalse(doc.Children().OfType<JProperty>().Any(x => x.Name.Equals("finishedAt")),
        "finishedAt should not be serialized because it is null");
    }

    [TestMethod]
    public void NullReferenceValueIsNotSerialized() {
      var entity = new EntityWithNullable
      {
        EntityWithNullableID = "e1",
        Name = null,
        Age = 10
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithNullable));
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      Assert.IsFalse(doc.Children().OfType<JProperty>().Any(x => x.Name.Equals("name")),
        "name field should not be serialized because it is null");
    }

    [TestMethod]
    public void ResolveDateTime() {
      string dateString = "2013-08-28T11:34:43.0065093Z";
      Type desiredType = typeof(DateTime);

      DateTime actual = (DateTime)Serializer.ResolveValue(new JValue(dateString), desiredType);
      DateTime expected = new DateTime(635132864830065093); // This ticks is the serialized date time exactly.

      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ResolveNullableDateTime() {
      string dateString = "2013-08-28T11:34:43.0065093Z";
      Type desiredType = typeof(Nullable<DateTime>);

      var actual = (Nullable<DateTime>)Serializer.ResolveValue(new JValue(dateString), desiredType);
      DateTime expected = new DateTime(635132864830065093); // This ticks is the serialized date time exactly.

      Assert.AreEqual(expected, actual.Value);
    }

    [TestMethod]
    public void ResolveNullValue() {
      string nullString = null;
      Type desiredType = typeof(DateTime);

      object actual = Serializer.ResolveValue(new JValue(nullString), desiredType);

      Assert.IsNull(actual);
    }

    [TestMethod]
    public void EntityWithToOneRef() {
      var entity = new EntityWithToOneAssociation
      {
        EntityWithToOneAssociationID = "entity10",
        AnotherEntityRef = new SimpleEntity
        {
          SimpleEntityID = "22",
          Name = "hello",
          Age = 22
        }
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithToOneAssociation));
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      JsonAssert.FieldEquals("22", doc, "anotherEntityRefID");
    }

    [TestMethod]
    public void EntityWithToOneAssociationWithIDField_OnlyToOneRefereceSet() {
      var entity = new EntityWithToOneAssociationWithIDField
      {
        EntityWithToOneAssociationWithIDFieldID = "entity10",
        AnotherEntityRef = new SimpleEntity
        {
          SimpleEntityID = "22",
          Name = "hello",
          Age = 22
        }
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithToOneAssociationWithIDField));
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      JsonAssert.FieldEquals("22", doc, "anotherEntityRefID");
    }

    [TestMethod]
    public void EntityWithToOneAssociationWithIDField_BothToOneRefereceAndIDPropertiesSet() {
      var entity = new EntityWithToOneAssociationWithIDField
      {
        EntityWithToOneAssociationWithIDFieldID = "entity10",
        AnotherEntityRefID = "33",
        AnotherEntityRef = new SimpleEntity
        {
          SimpleEntityID = "22",
          Name = "hello",
          Age = 22
        }
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithToOneAssociationWithIDField));
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      JsonAssert.FieldEquals("22", doc, "anotherEntityRefID");
    }

    [TestMethod]
    public void EntityWithToOneAssociationWithIDField_OnlyIDPropertySet() {
      var entity = new EntityWithToOneAssociationWithIDField
      {
        EntityWithToOneAssociationWithIDFieldID = "entity10",
        AnotherEntityRefID = "33",
        AnotherEntityRef = null
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithToOneAssociationWithIDField));
      JObject doc = subject.Serialize("2-update", "person-with-friends", entity);

      JsonAssert.FieldEquals("33", doc, "anotherEntityRefID");
    }

    [TestMethod]
    public void EntityWithEmbedded_Serialze() {
      var entity = new EntityWithEmbeddedAddress
      {
        PeronName = "Jhon",
        Address = new Address
        {
          Street = "Infinate Loop St.",
          Number = 56
        }
      };

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithEmbeddedAddress));
      JObject doc = subject.Serialize("2-update", "person-with-address", entity);

      Assert.AreEqual("Infinate Loop St.", doc["address"]["street"].Value<string>());
      Assert.AreEqual("56", doc["address"]["number"].Value<string>());
    }

    [TestMethod]
    public void EntityWithEmbedded_Deserialize() {
      string serializedEntity =
        @"{
  ""$type"": ""person-with-address"",
  ""_rev"": ""2-update"",
  ""address"": {
    ""street"": ""Infinate Loop St."",
    ""number"": 56
  },
  ""peronName"": ""Jhon""
}";

      JObject jobj = JObject.Parse(serializedEntity);

      Serializer subject = SerializationTestHelper.CreateSerializer(typeof(EntityWithEmbeddedAddress));
      var entity = (EntityWithEmbeddedAddress)subject.CreateProxy(jobj, typeof(EntityWithEmbeddedAddress), "e1", null, null, true);

      Assert.AreEqual<string>("Infinate Loop St.", entity.Address.Street);
      Assert.AreEqual<int>(56, entity.Address.Number);
    }

    private static class JArrayHelper {

      public static T[] ArrayOf<T>(JArray jArray) {
        return jArray.Select(jt => jt.Value<T>()).ToArray();
      }
    }
  }
}
