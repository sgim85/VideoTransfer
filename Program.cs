using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace VideoDeDublicator
{
    class Program
    {
        static void Main(string[] args)
        {
            string inFolder = @"D:\Archive";
            string externalDrive = @"Y:\Plex Media\Other Videos\Docs";
            string hashLocation = @"C:\Users\Samson\OneDrive\Documents\hashes.txt";
            string failedFiles = @"C:\Users\Samson\OneDrive\Documents\failedFiles.txt";

            CompareVids(inFolder, externalDrive, hashLocation, failedFiles);
        }

        static void CompareVids(string inFolder, string externalDrive, string hashLocation, string failedFiles)
        {
            Stack<string> stack = new Stack<string>();
            stack.Push(inFolder);
            HashSet<string> visitedFolders = new HashSet<string>();
            HashSet<string> filesToMove = new HashSet<string>();
            HashSet<string> md5Hashes = new HashSet<string>();

            var previousHashes = System.IO.File.ReadAllLines(hashLocation);
            foreach(var h in previousHashes)
                md5Hashes.Add(h);

            int counter1 = 0, counter2 = 0;

            while (stack.Count > 0)
            {
                var path = stack.Pop();

                if (visitedFolders.Contains(path))
                    continue;

                foreach (string fileOrFol in Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        //string contents = File.ReadAllText(fileOrFol);
                        var attr = File.GetAttributes(fileOrFol);

                        if (attr.HasFlag(FileAttributes.Directory))
                        {
                            stack.Push(fileOrFol);
                            visitedFolders.Add(fileOrFol);
                            Console.WriteLine($"Stack folder: {fileOrFol}");
                        }
                        else
                        {
                            var hash = GenerateHash(fileOrFol);
                            Console.WriteLine($"{++counter1}: Processing file: {fileOrFol}");

                            if (!md5Hashes.Contains(hash))
                            {
                                md5Hashes.Add(hash);
                                filesToMove.Add(fileOrFol);
                                Console.WriteLine($"{++counter2}: Processing hash: {hash}");
                            }
                        }
                    }
                    catch(Exception)
                    {
                        using(StreamWriter sw = System.IO.File.AppendText(failedFiles))
                        {
                            sw.WriteLine(fileOrFol);
                        }
                    }
                }
            }

            foreach (string s in filesToMove)
            {
                string[] directories = s.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                directories[0] = externalDrive;
                //directories[1] += "_New";
                var newPath = Path.Combine(directories);

                // Create dir if not exist
                FileInfo file = new FileInfo(newPath);
                file.Directory.Create();

                // Copy file to new location.
                File.Copy(s, newPath, true);
            }

            System.IO.File.WriteAllLines(hashLocation, md5Hashes.Select(a => a.ToString()));
        }

        static string GenerateHash(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            string hexVal = "";

            byte[] fileDate = File.ReadAllBytes(filePath);
            byte[] hash = SHA1.Create().ComputeHash(fileDate);

            foreach (byte b in hash)
            {
                hexVal = b.ToString("X").ToLower();
                sb.Append(hexVal);
            }
            return sb.ToString();
        }
    }
}
