using System;
using System.Reflection;

namespace CouchPotato.Annotations {
  /// <summary>
  /// Identify a property as association with another entity.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
  public sealed class AssociationAttribute : Attribute {

    private readonly string inversePropertyName;

    /// <summary>
    /// Initialize new instance of AssociationAttribute which indicate the property
    /// is an inverse assoication property of OneToMany.
    /// </summary>
    public AssociationAttribute() {
    }

    /// <summary>
    /// Initialize new instance of AssoicationAttribute which indicate the property
    /// is an inverse assoication property of ManyToMany
    /// </summary>
    /// <param name="inversePropertyName"></param>
    public AssociationAttribute(string inversePropertyName) {
      this.inversePropertyName = inversePropertyName; 
    }

    public string InversePropertyName {
      get { return inversePropertyName; }
    }
    
    internal static AssociationAttribute GetSingle(PropertyInfo prop) {
      var attr = prop.GetCustomAttribute<AssociationAttribute>();
      return attr;
    }
  }
}
