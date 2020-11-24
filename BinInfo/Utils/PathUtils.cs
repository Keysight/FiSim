using System;
using System.IO;

namespace BinInfo.Utils {
    public class PathUtils {
        // https://stackoverflow.com/questions/3855956/check-if-an-executable-exists-in-the-windows-path
        public static bool ExistsOnPath(string fileName) {
            return GetFullPath(fileName) != null;
        }

        public static string GetFullPath(string fileName) {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            
            foreach (var path in values.Split(Path.PathSeparator)) {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            
            return null;
        }
    }
}