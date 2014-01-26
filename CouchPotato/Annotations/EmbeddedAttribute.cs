using System;

namespace CouchPotato.Annotations {
  /// <summary>
  /// Identify a property as embedded entity.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
  public class EmbeddedAttribute : Attribute {
  }
}
