﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace ClusterioLibSharp.NodeLibs
{
  // Source: https://stackoverflow.com/a/22942728/2398014
  public static class DynamicHelpers
  {
    public static dynamic CombineDynamics(object object1, object object2)
    {
      IDictionary<string, object> dictionary1 = GetKeyValueMap(object1);
      IDictionary<string, object> dictionary2 = GetKeyValueMap(object2);

      var result = new ExpandoObject();

      var d = result as IDictionary<string, object>;
      foreach (var pair in dictionary1.Concat(dictionary2))
      {
        d[pair.Key] = pair.Value;
      }

      return result;
    }

    private static IDictionary<string, object> GetKeyValueMap(object values)
    {
      if (values == null)
      {
        return new Dictionary<string, object>();
      }

      var map = values as IDictionary<string, object>;
      if (map != null)
      {
        return map;
      }

      map = new Dictionary<string, object>();
      foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
      {
        map.Add(descriptor.Name, descriptor.GetValue(values));
      }

      return map;
    }
  }
}
