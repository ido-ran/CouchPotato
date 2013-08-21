using CouchPotato.Odm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CouchPotato.Test {
  /// <summary>
  /// Assert for CouchPotato.
  /// </summary>
  internal static class CouchPotatoAssert {

    /// <summary>
    /// Assert the modified documents count in the DocumentManager.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="expectedCount"></param>
    /// <param name="message"></param>
    public static void ModifiedDocumentsCount(CouchDBContext context, int expectedCount, string message = null) {
      int modifiedDocCount = context.DocumentManager.GetModifiedDocuments().Length;
      Assert.AreEqual(expectedCount, modifiedDocCount, message);
    }
  }
}
