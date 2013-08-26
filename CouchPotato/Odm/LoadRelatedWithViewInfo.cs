using System.Reflection;

namespace CouchPotato.Odm {
  /// <summary>
  /// Contain information for loading related entities using view(s).
  /// </summary>
  internal class LoadRelatedWithViewInfo {
    private readonly PropertyInfo propInfo;
    private readonly string viewName;

    public LoadRelatedWithViewInfo(PropertyInfo propInfo, string viewName) {
      this.propInfo = propInfo;
      this.viewName = viewName;
    }

    public PropertyInfo PropertyInfo {
      get { return propInfo; }
    }

    public string ViewName {
      get { return viewName; }
    }

  }
}
