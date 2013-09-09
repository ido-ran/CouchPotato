using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Test {
  /// <summary>
  /// Json objects assertions.
  /// </summary>
  internal static class JsonAssert {
    /// <summary>
    /// Check that a field in the JSON object equals the expected value.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="doc"></param>
    /// <param name="fieldName"></param>
    internal static void FieldEquals(string expected, JObject doc, string fieldName) {
      Assert.IsTrue(doc.Children().OfType<JProperty>().Any(x => x.Name.Equals(fieldName)), "Fail to find field name " + fieldName);
      Assert.AreEqual<string>(expected, doc.Value<string>(fieldName));
    }
  }
}
