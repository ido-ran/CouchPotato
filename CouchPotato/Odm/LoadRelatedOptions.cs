using System;
using System.Reflection;

namespace CouchPotato.Odm {
  /// <summary>
  /// Contain the options specified to LoadRelated method.
  /// </summary>
  internal class LoadRelatedOptions {
    private readonly PropertyInfo[] toOne;
    private readonly PropertyInfo[] toOneExist;
    private readonly PropertyInfo[] toManyDirect;
    private readonly LoadRelatedWithViewInfo[] toManyView;

    public LoadRelatedOptions(
      PropertyInfo[] toOne,
      PropertyInfo[] toOneExist, 
      PropertyInfo[] toManyDirect,
      LoadRelatedWithViewInfo[] toManyView) {

      this.toOne = toOne;
      this.toOneExist = toOneExist;
      this.toManyDirect = toManyDirect;
      this.toManyView = toManyView;
    }

    /// <summary>
    /// ToOne association properties to load.
    /// </summary>
    public PropertyInfo[] ToOne {
      get { return toOne; }
    }

    /// <summary>
    /// ToOne assoication properties of entities already loaded.
    /// </summary>
    public PropertyInfo[] ToOneExist {
      get { return toOneExist; }
    }

    /// <summary>
    /// Direct ToOne association.
    /// </summary>
    public PropertyInfo[] ToManyDirect {
      get { return toManyDirect; }
    }

    /// <summary>
    /// Inverse ToMany assoication to load.
    /// Tuple of property info to entity IDs to load.
    /// </summary>
    public LoadRelatedWithViewInfo[] ToManyView {
      get { return toManyView; }
    }
  }
}
