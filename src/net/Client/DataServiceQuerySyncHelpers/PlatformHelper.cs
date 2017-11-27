using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.WindowsAzure.MediaServices.Client.DataServiceQuerySyncHelpers
{
    public static class PlatformHelper
    {
        /// <summary>Gets all public static methods for a type.</summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>Enumerable of all public static methods for the specified type.</returns>
        internal static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type) 
            => type.GetRuntimeMethods().Where(m => m.IsPublic && m.IsStatic);
    }
}