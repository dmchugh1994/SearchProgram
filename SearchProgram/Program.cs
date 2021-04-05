﻿using System;
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
            string searchValue = "@gmail.com";

            SearchAsync(searchValue);
        }
        public static async void SearchAsync(string searchValue)
        {
            try
            {
                Console.WriteLine("Loading all files...");

                IEnumerable fileArray = Directory.EnumerateFiles(@"E:\", "*.txt", SearchOption.AllDirectories);

                foreach (string file in fileArray)
                {
                    await Task.Run(() =>
                    {
                        ReadAllLinesAsync(file, searchValue);
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static Task<string[]> ReadAllLinesAsync(string path, string searchValue)
        {
            return ReadAllLinesAsync(path, Encoding.UTF8, searchValue);
        }

        public static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, string searchValue)
        {
            var lines = new List<string>();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    Search(line, searchValue);

                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }

        public static void Search(string line, string searchValue)
        {
                if (line.Contains(searchValue))
                {
                    Console.WriteLine("  " + line);
                }
        }
    }
}