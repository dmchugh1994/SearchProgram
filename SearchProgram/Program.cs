using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace SearchProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            string searchValue = "@gmail.com";

            SearchAsync(searchValue);

        }

        public static async void SearchAsync(string searchValue)
        {
            try
            {
                Console.WriteLine("Loading all Databases...");

                IEnumerable fileArray = Directory.EnumerateFiles(@"E:\", "*.txt", SearchOption.AllDirectories);

                using (StreamWriter SW = new StreamWriter("C:/temp/testing.txt"))
                {
                    foreach (string file in fileArray)
                    {
                        await Task.Run(() =>
                        {
                            SearchProcess(file, searchValue);
                        });
                    }

                    Task.WaitAll();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void SearchProcess(string file, string searchValue)
        {
            Console.WriteLine(" Searching: " + file);
            using StreamReader sr = new(file);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line.Contains(searchValue))
                {
                    Console.WriteLine("  " + line);
                }
            }
        }
    }
}