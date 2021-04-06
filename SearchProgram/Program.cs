using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearchProgram
{
    class Program
    {
        private const int DefaultBufferSize = 4096;
        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        private const string fmt = "000000.##";

        private static readonly SemaphoreSlim listSync = new(1, 1);

        static async Task Main(string[] args)
        {
            string searchValue = "@gmail.com";

            await SearchAsync(searchValue);
        }
        public static async Task SearchAsync(string searchValue)
        {
            try
            {
                int fileCount = 0;
                int currentlyScanned = 0;
                int currentResults = 0;

                var lines = new List<string>();

                Console.WriteLine("Loading all files...");
                var fileArray = Directory.EnumerateFiles(@"E:\", "*.txt", SearchOption.AllDirectories);

                foreach (var file in fileArray)
                    fileCount++;

                Console.WriteLine("Loaded " + fileCount + " files...");

                await fileArray.ParallelForEachAsync(1000, async file =>
                {
                    currentlyScanned++;

                    Console.WriteLine(currentlyScanned.ToString(fmt) + "/" + fileCount + "  ||  Currently Searching " + Path.GetFileName(file) + "...");

                    var matchingLines = await ReadAllLinesAsync(file, searchValue, Path.GetFileName(file));

                    await listSync.WaitAsync();
                    try
                    {
                        lines.AddRange(matchingLines);
                    }
                    finally
                    {
                        listSync.Release();
                    }
                });

                Console.WriteLine("Results: " + lines.Count);
                //foreach (string line in lines)
                //{
                //    Console.WriteLine(line);
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static Task<List<string>> ReadAllLinesAsync(string path, string searchValue, string fileName)
        {
            return ReadAllLinesAsync(path, Encoding.UTF8, searchValue, fileName);
        }

        public static async Task<List<string>> ReadAllLinesAsync(string path, Encoding encoding, string searchValue, string fileName)
        {
            var lines = new List<string>();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    string newLine = Search(line, searchValue, fileName);

                    if (!String.IsNullOrEmpty(newLine))
                    {
                        lines.Add(newLine);

                        Console.WriteLine("   " + fileName + " -- " + newLine);
                    }
                }
            }

            return lines;
        }

        public static string Search(string line, string searchValue, string fileName)
        {
            if (line.Contains(searchValue))
            {
                return line;
            }
            else
                return String.Empty;
        }
    }
}