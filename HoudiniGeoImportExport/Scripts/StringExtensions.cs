using System;
using System.IO;

namespace Houdini.GeoImportExport
{
    /// <summary>
    /// Useful string extensions.
    /// </summary>
    public static class StringExtensions 
    {
        public static string RemoveSuffix(this string name, string suffix)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(suffix))
                return name;

            if (!name.EndsWith(suffix))
                return name;

            return name.Substring(0, name.Length - suffix.Length);
        }
    
        public static string RemoveSuffix(this string name, char suffix)
        {
            return name.RemoveSuffix(suffix.ToString());
        }
        
        /// <summary>
        /// Converts the slashes to be consistent.
        /// </summary>
        public static string ToUnityPath(this string name)
        {
            return name.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
