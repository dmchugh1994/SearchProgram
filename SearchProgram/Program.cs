using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SearchProgram
{
    class Program
    {
        private const int DefaultBufferSize = 4096;
        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

        static void Main(string[] args)
        {
            string searchValue = "jenny";

            SearchAsync(searchValue);
        }
        public static async void SearchAsync(string searchValue)
        {
            try
            {
                int fileCount = 0;
                string[] lineArray = Array.Empty<string>();

                Console.WriteLine("Loading all files...");
                IEnumerable fileArray = Directory.EnumerateFiles(@"E:\", "*.txt", SearchOption.AllDirectories);

                foreach (var file in fileArray)
                    fileCount++;

                Console.WriteLine("Loaded " + fileCount + " files...");

                foreach (string file in fileArray)
                {
                    Console.WriteLine("Currently Searching " + Path.GetFileName(file) + "...");
                    lineArray = await ReadAllLinesAsync(file, searchValue, Path.GetFileName(file));
                }

                Console.WriteLine("Results: " + lineArray.Length);
                //foreach (string line in lineArray)
                //{
                //    Console.WriteLine(line);
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static Task<string[]> ReadAllLinesAsync(string path, string searchValue, string fileName)
        {
            return ReadAllLinesAsync(path, Encoding.UTF8, searchValue, fileName);
        }

        public static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, string searchValue, string fileName)
        {
            var lines = new List<string>();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    string newLine = Search(line, searchValue, fileName);

                    if (String.IsNullOrEmpty(newLine))
                    {
                        lines.Add(newLine);
                    }
                }
            }

            return lines.ToArray();
        }

        public static string Search(string line, string searchValue, string fileName)
        {
            if (line.Contains(searchValue))
            {
                Console.WriteLine("   " + fileName + " -- " + line);
                return line;
            }
            else
                return String.Empty;
        }
    }
}