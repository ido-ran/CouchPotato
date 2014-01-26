using System;
using System.Runtime.Serialization;

namespace CouchPotato.Odm {
  /// <summary>
  /// Thrown when save changes fail to complete successfully.
  /// </summary>
  [Serializable]
  public class SaveChangesException : Exception {

    private const string DefaultMessage = "Fail to save changes";

    public SaveChangesException() : base(DefaultMessage) { }
    public SaveChangesException(Exception inner) : base(DefaultMessage, inner) { }
    protected SaveChangesException(SerializationInfo info, StreamingContext context)
      : base(info, context) { }
  }
}
