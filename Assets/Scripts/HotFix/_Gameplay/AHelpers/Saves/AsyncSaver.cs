using System;
using System.IO;

namespace MarbleHero
{
    public class AsyncSaver
    {
        public static void save(string path, string data)
        {
            try
            {
                var dirPath = Path.GetDirectoryName(path);
                if (dirPath != null && !Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                File.WriteAllTextAsync(path, data);
            }
            catch (Exception e)
            {
                return;
            }
        }
    }
}