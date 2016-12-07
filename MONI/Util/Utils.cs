using System;
using System.IO;

namespace MONI.Util
{
    public class Utils
    {
        public static bool CanCreateFile(string dir)
        {
            string file = Path.Combine(dir, Guid.NewGuid().ToString() + ".tmp");
            // perhaps check File.Exists(file), but it would be a long-shot...
            bool canCreate;
            try
            {
                using (File.Create(file)) { }
                File.Delete(file);
                canCreate = true;
            }
            catch
            {
                canCreate = false;
            }
            return canCreate;
        }

        public static string MoniAppDataPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var moniAppData = Path.Combine(appData, "moni");
            return moniAppData;
        }

        public static string PatchFilePath(string path)
        {
            return path.Replace("#{appdata}", MoniAppDataPath()).Replace("#{userhome}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }
    }
}