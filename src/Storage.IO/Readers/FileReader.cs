using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Readers
{
    public static class FileReader
    {
        public static string[] TryReadAllLines(string path)
        {
            try
            {
                return File.ReadAllLines(path);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
