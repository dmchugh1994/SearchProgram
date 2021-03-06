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

        private static readonly SemaphoreSlim listSync = new(1, 1);

        private static int currentResults = 0;
        private static string searchValue;
        private static string databaseLocation;
        private static int minWorker, minIOC, maxWorker, maxIOC;

        private static string defaultSearchValue = "gmail.com";
        private static string defaultDatabaseLocation = "E:/";

        private static string outputFile = "C:/temp/Output" + DateTime.Now.ToString("yyyyMMdd'T'HHmmss") + ".csv";

        static async Task Main(string[] args)
        {
            ThreadPool.GetMinThreads(out minWorker, out minIOC);
            ThreadPool.SetMinThreads(400, minIOC);
            ThreadPool.GetMaxThreads(out maxWorker, out maxIOC);
            ThreadPool.SetMaxThreads(2000, maxIOC);

            await SearchAsync();
        }
        public static async Task SearchAsync()
        {
            try
            {
                int fileCount = 0;
                int currentlyScanned = 0;

                var lines = new List<string>();

                Console.Write($"Enter database location (Default: '{defaultDatabaseLocation}'): ");
                var tempDatabaseLocationInput = Console.ReadLine();
                if (String.IsNullOrEmpty(tempDatabaseLocationInput))
                {
                    databaseLocation = defaultDatabaseLocation;
                }
                else
                {
                    databaseLocation = tempDatabaseLocationInput;
                }

                Console.Write("Enter search value (Default: '" + defaultSearchValue + "'): ");
                var tempSearchValueInput = Console.ReadLine();
                if (String.IsNullOrEmpty(tempSearchValueInput))
                {
                    searchValue = defaultSearchValue;
                }
                else
                {
                    searchValue = tempSearchValueInput;
                }

                Console.WriteLine("");
                Console.WriteLine("Loading all files...");
                Console.WriteLine("");

                var fileArray = Directory.EnumerateFiles(@databaseLocation, "*.txt", SearchOption.AllDirectories);

                foreach (var file in fileArray)
                    fileCount++;

                Console.WriteLine("Loaded " + fileCount + " files...");

                using (StreamWriter writer = new(outputFile, append: true))
                {
                    await fileArray.ParallelForEachAsync(400, async file =>
                    {
                        Interlocked.Add(ref currentlyScanned, 1);

                        Console.WriteLine($"{currentlyScanned.ToString("000000.##")}/{fileCount}({currentResults})  ||  Currently Searching {Path.GetFileName(file)}...");

                        var matchingLines = await ReadAllLinesAsync(file, searchValue, Path.GetFileName(file));

                        await listSync.WaitAsync();
                        try
                        {
                            foreach (var line in matchingLines)
                            {
                                await writer.WriteLineAsync(line);
                            }

                            lines.AddRange(matchingLines);
                        }
                        finally
                        {
                            listSync.Release();
                        }
                    });
                }
                
                Console.WriteLine("");
                Console.WriteLine("Results: " + lines.Count);
                Console.WriteLine("");
                Console.WriteLine("Output file can be found at: " + outputFile);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unhandled exception in SearchAsync(). {e.Message}");
                Console.WriteLine(e.StackTrace);
            }

            Console.WriteLine("");
            Console.WriteLine("Press Enter to close program...");
            Console.ReadLine();
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
                        newLine = newLine.Replace(",", "");

                        lines.Add(fileName + "," + newLine);

                        Interlocked.Add(ref currentResults, 1);

                        Console.WriteLine("   Current Results " + currentResults.ToString("0000.##") + "  ||  " + fileName + " -- " + newLine);
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