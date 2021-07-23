﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Heinermann.ONIProfiler
{
  public class ProfileUtils
  {
    public static Assembly GetAssemblyByName(string name)
    {
      return AppDomain.CurrentDomain.GetAssemblies().
             SingleOrDefault(assembly => assembly.GetName().Name == name);
    }

    static readonly string[] BANNED_TYPES = {
      "Expectations",
      "KleiAccount",
      "KleiMetrics",
      "ThreadedHttps`1[T]",
      "System.Enum",
      "System.Number",
      "System.SharedStatics",
      "System.IO.WindowsWatcher",
      "System.Buffers.Binary.BinaryPrimitives",
      "System.Environment",
      "LibNoiseDotNet.",
      "Mono.",
      "System.Reflection.",
      "System.IO.IsolatedStorage.",
      "System.Threading.",
      "System.Security.",
      "System.Runtime.",
      "System.Diagnostics.",
      "System.Configuration.",
      "System.IO.Ports.",
      "System.IO.Compression.",
      "System.Net.",
      "System.CodeDom.",
      "Microsoft."
    };

    static readonly string[] PROFILED_ASSEMBLIES = {
      "Assembly-CSharp",
      "UnityEngine",
      "UnityEngine.CoreModule"
    };

    public static IEnumerable<MethodBase> GetTargetMethodsForAssembly(string assemblyName)
    {
      Assembly assembly = GetAssemblyByName(assemblyName);
      if (assembly == null)
      {
        Debug.LogError("Failed to find assembly");
      }

      var assemblyTypes = assembly.GetTypes()
        .Where(type =>
          type.IsClass &&
          !type.Attributes.HasFlag(TypeAttributes.HasSecurity) &&
          !type.Attributes.HasFlag(TypeAttributes.Import) &&
          !type.Attributes.HasFlag(TypeAttributes.Interface) &&
          !type.IsImport &&
          !type.IsInterface &&
          !type.IsSecurityCritical &&
          !BANNED_TYPES.Any(type.FullName.StartsWith)
        );

      var assemblyMethods = assemblyTypes
        .SelectMany(type => AccessTools.GetDeclaredMethods(type))
        .Where(method => {
          foreach (object attr in method.GetCustomAttributes(false))
          {
            if ((attr is DllImportAttribute) ||
              (attr is MethodImplAttribute) ||
              (attr is CLSCompliantAttribute) ||
              (attr is SecurityCriticalAttribute) ||
              (attr is ObsoleteAttribute)
            )
            {
              return false;
            }
          }

          if (method.GetMethodBody() == null || method.Name.Equals("ReadUInt64")) return false;

          return !method.ContainsGenericParameters &&
          !method.IsAbstract &&
          !method.IsVirtual &&
          !method.GetMethodImplementationFlags().HasFlag(MethodImplAttributes.Native) &&
          !method.GetMethodImplementationFlags().HasFlag(MethodImplAttributes.Unmanaged) &&
          !method.GetMethodImplementationFlags().HasFlag(MethodImplAttributes.InternalCall) &&
          !method.Attributes.HasFlag(MethodAttributes.PinvokeImpl) &&
          !method.Attributes.HasFlag(MethodAttributes.Abstract) &&
          !method.Attributes.HasFlag(MethodAttributes.UnmanagedExport);
        });
        
      return assemblyMethods.Cast<MethodBase>();
    }

    public static IEnumerable<MethodBase> GetTargetMethods()
    {
      return PROFILED_ASSEMBLIES.SelectMany(GetTargetMethodsForAssembly);
    }
    public static long ticksToNanoTime(long ticks)
    {
      return 10000L * ticks / TimeSpan.TicksPerMillisecond * 100L;
    }

  }
}
