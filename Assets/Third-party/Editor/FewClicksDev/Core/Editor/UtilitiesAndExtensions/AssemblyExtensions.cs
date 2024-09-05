namespace FewClicksDev.Core
{
    using System;
    using System.Reflection;
    using UnityEngine;

    public static class AssemblyExtensions
    {
        public const string ASSEMBLY_EXTENSIONS = "ASSEMBLY EXTENSIONS";

        public static readonly Color MAIN_COLOR = new Color(0.06051977f, 0.5680292f, 0.754717f, 1f);

        public static object InvokeInternalStaticMethod(Type _objectType, string _methodName, params object[] _params)
        {
            var _method = _objectType.GetMethod(_methodName, BindingFlags.NonPublic | BindingFlags.Static);

            if (_method == null)
            {
                BaseLogger.Error(ASSEMBLY_EXTENSIONS, $"{_methodName} doesn't exist in the {_objectType} type!", MAIN_COLOR);
                return null;
            }

            return _method.Invoke(null, _params);
        }
    }
}
