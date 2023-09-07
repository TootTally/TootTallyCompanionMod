using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace TootTally.Utils.Helpers
{
    public static class FileHelper
    {
        public static void WriteJsonToFile(string dirName, string fileName, string jsonString)
        {
            try
            {
                TootTallyLogger.DebugModeLog("Creating MemoryStream Buffer for replay creation.");
                using (var memoryStream = new MemoryStream())
                {
                    TootTallyLogger.DebugModeLog("Creating ZipArchive for replay creation.");
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        var zipFile = zipArchive.CreateEntry(fileName);

                        TootTallyLogger.DebugModeLog("Writing Zipped replay to MemoryStream Buffer.");
                        using (var entry = zipFile.Open())
                        using (var sw = new StreamWriter(entry))
                        {
                            sw.Write(jsonString);
                        }
                    }

                    TootTallyLogger.DebugModeLog("Writing MemoryStream to File.");
                    using (var fileStream = new FileStream(dirName + fileName, FileMode.CreateNew))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                TootTallyLogger.LogError(e.Message);
            }

        }

        public static string ReadJsonFromFile(string dirName, string fileName)
        {
            try
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
            catch (Exception e)
            {
                TootTallyLogger.LogError(e.Message);
                return null;
            }
        }

        public static void WriteBytesToFile(string dirName, string fileName, byte[] bytes)
        {
            if (File.Exists(dirName + fileName)) return;

            File.Create(dirName + fileName).Close();

            File.WriteAllBytes(dirName + fileName, bytes);
        }

        public static void ExtractZipToDirectory(string source, string destination)
        {
            if (File.Exists(source))
                ZipFile.ExtractToDirectory(source, destination, true);
        }

        public static void DeleteFile(string dirName, string fileName)
        {
            if (File.Exists(dirName+fileName))
                File.Delete(dirName+fileName);
        }

        //Taken from https://stackoverflow.com/a/14488941
        private static readonly string[] SizeSuffixes =
                   { "b", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} b", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
