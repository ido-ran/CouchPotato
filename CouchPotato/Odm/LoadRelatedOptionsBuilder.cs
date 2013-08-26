using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchPotato.Odm {
  /// <summary>
  /// LoadRelatedOptions builder.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class LoadRelatedOptionsBuilder<T> {

    private HashSet<PropertyInfo> toOne;
    private HashSet<PropertyInfo> toOneExist;
    private HashSet<PropertyInfo> toManyDirect;
    private HashSet<LoadRelatedWithViewInfo> toManyInverseWithView;

    public LoadRelatedOptionsBuilder() {
      toOne = new HashSet<PropertyInfo>();
      toOneExist = new HashSet<PropertyInfo>();
      toManyDirect = new HashSet<PropertyInfo>();
      toManyInverseWithView = new HashSet<LoadRelatedWithViewInfo>();
    }

    /// <summary>
    /// Load ToOne association.
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyExpression"></param>
    /// <returns></returns>
    public LoadRelatedOptionsBuilder<T> One<TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
      var propInfo = GetPropertyInfo<TProperty>(propertyExpression);
      toOne.Add(propInfo);

      return this;
    }

    /// <summary>
    /// Load ToOne assoication which should already exist in the context.
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyExpression"></param>
    /// <returns></returns>
    public LoadRelatedOptionsBuilder<T> OneExist<TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
      var propInfo = GetPropertyInfo<TProperty>(propertyExpression);
      toOneExist.Add(propInfo);

      return this;
    }

    private static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
      var memberInfo = (MemberExpression)propertyExpression.Body;
      var propInfo = (PropertyInfo)memberInfo.Member;
      return propInfo;
    }

    /// <summary>
    /// Load ToMany direct association.
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyExpression"></param>
    /// <returns></returns>
    public LoadRelatedOptionsBuilder<T> Many<TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
      var propInfo = GetPropertyInfo<TProperty>(propertyExpression);
      toManyDirect.Add(propInfo);

      return this;
    }

    /// <summary>
    /// Load ToMany inverse association using a view.
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyExpression"></param>
    /// <param name="viewName"></param>
    /// <returns></returns>
    public LoadRelatedOptionsBuilder<T> ManyView<TProperty>(
      Expression<Func<T, TProperty>> propertyExpression, string viewName) {

      PropertyInfo propInfo = GetPropertyInfo<TProperty>(propertyExpression);
      toManyInverseWithView.Add(new LoadRelatedWithViewInfo(propInfo, viewName));

      return this;
    }

    internal LoadRelatedOptions Build() {
      return new LoadRelatedOptions(
        toOne.ToArray(), 
        toOneExist.ToArray(),
        toManyDirect.ToArray(), 
        toManyInverseWithView.ToArray());
    }
  }
}
