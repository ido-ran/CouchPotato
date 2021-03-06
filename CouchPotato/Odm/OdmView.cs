﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CouchPotato.CouchClientAdapter;
using Newtonsoft.Json.Linq;

namespace CouchPotato.Odm {
  public class OdmView<T> : IEnumerable<T> {
    private readonly CouchDBContextImpl couchDBContext;
    private readonly string viewName;
    private CouchViewOptions options;
    private List<Tuple<string, object>> assoicateCollectionsToLoad;

    public OdmView(CouchDBContextImpl couchDBContext, string viewName) {
      this.couchDBContext = couchDBContext;
      this.viewName = viewName;

      assoicateCollectionsToLoad = new List<Tuple<string, object>>();
      options = new CouchViewOptions
      {
        IncludeDocs = true
      };
    }

    public OdmView<T> AssociatedCollection<TProperty>(
      Expression<Func<T, TProperty>> associatedCollectionExpression,
      object associatedKeyPart) {

      var me = (MemberExpression)associatedCollectionExpression.Body;
      string associateCollectionPropName = me.Member.Name;
      assoicateCollectionsToLoad.Add(Tuple.Create(associateCollectionPropName, associatedKeyPart));

      return this;
    }

    public OdmView<T> Key(params object[] key) {
      foreach (object keyPart in key) {
        options.Key.Add(keyPart);
      }
      return this;
    }

    public OdmView<T> Keys(object[] keys) {
      options.Keys.AddRange(keys);
      return this;
    }

    public OdmView<T> StartKey(params object[] values) {
      options.StartKey = new List<object>(values);
      return this;
    }

    public OdmView<T> EndKey(params object[] values) {
      options.EndKey = new List<object>(values);
      return this;
    }

    public IEnumerator<T> GetEnumerator() {
      return ExecuteView(options).Entities.OfType<T>().GetEnumerator();
    }

    private EntitiesProcessResult ExecuteView(CouchViewOptions viewOptions) {
      JToken[] rows = couchDBContext.ClientAdaper.GetViewRows(viewName, viewOptions);
      var processingOptions = new OdmViewProcessingOptions(assoicateCollectionsToLoad);

      EntitiesProcessResult processResult = couchDBContext.Process(rows, processingOptions);
      return processResult;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    /// <summary>
    /// Reverse the order of the results.
    /// </summary>
    /// <returns></returns>
    public OdmView<T> Descending() {
      options.Descending = true;
      return this;
    }

    /// <summary>
    /// Limit the number of results from the view.
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    public OdmView<T> Limit(int limit) {
      options.Limit = limit;
      return this;
    }
  }
}
