using System.IO;
using System.IO.Compression;
using System.Text;

namespace TootTally.Utils.Helpers
{
    public static class FileHelper
    {
        public static void WriteJsonToFile(string dirName, string fileName, string jsonString)
        {
            Plugin.DebugModeLog("Creating MemoryStream Buffer for replay creation.");
            using (var memoryStream = new MemoryStream())
            {
                Plugin.DebugModeLog("Creating ZipArchive for replay creation.");
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    var zipFile = zipArchive.CreateEntry(fileName);

                    Plugin.DebugModeLog("Writing Zipped replay to MemoryStream Buffer.");
                    using (var entry = zipFile.Open())
                    using (var sw = new StreamWriter(entry))
                    {
                        sw.Write(jsonString);
                    }
                }

                Plugin.DebugModeLog("Writing MemoryStream to File.");
                using (var fileStream = new FileStream(dirName + fileName, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
        }

        public static string ReadJsonFromFile(string dirName, string fileName)
        {
            string jsonString;
            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = new FileStream(dirName + fileName, FileMode.Open))
                {
                    fileStream.CopyTo(memoryStream);
                }

                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true))
                {
                    var zipFile = zipArchive.GetEntry(zipArchive.Entries[0].Name);

                    using (var entry = zipFile.Open())
                    using (var sr = new StreamReader(entry))
                    {
                        jsonString = sr.ReadToEnd();
                    }

                }
            }
            return jsonString;
        }
    }
}
