namespace VS_Artifact_Cleaner
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    class Program
    {
        #region Fields

        private static ConsoleColor _defaultForegroundColor;
        private static ConsoleColor _directoryColor;
        private static ConsoleColor _fileColor;
        private static ConsoleColor _deleteColor;

        #endregion //Fields

        #region Methods

        static void Main(string[] args)
        {
            try
            {
                Maximize();

                _defaultForegroundColor = Console.ForegroundColor;
                _directoryColor = ConsoleColor.Green;
                _fileColor = ConsoleColor.White;
                _deleteColor = ConsoleColor.Red;

                string executingDirectory = GetExecutingDirectory();
                List<string> directoryFilters = GetDirectoryFilters();
                List<string> fileFilters = GetFileFilters();

                Console.WriteLine("*** Visual Studio Artifact Cleaner ***");
                Console.WriteLine();

                Console.WriteLine("Searching for directory names:");
                Console.WriteLine();
                Console.ForegroundColor = _directoryColor;
                directoryFilters.ForEach(p => Console.WriteLine(p));
                List<string> directories = Directory.GetDirectories(executingDirectory, "*", SearchOption.AllDirectories)
                    .Where(p => directoryFilters.Contains(Path.GetFileName(p))).ToList();
                Console.WriteLine();
                Console.ForegroundColor = _defaultForegroundColor;

                Console.WriteLine("Searching for files with extensions:");
                Console.WriteLine();
                Console.ForegroundColor = _fileColor;
                fileFilters.ForEach(p => Console.WriteLine(p));
                List<string> files = Directory.GetFiles(executingDirectory, "*.*", SearchOption.AllDirectories)
                    .Where(p => fileFilters.Contains(Path.GetExtension(p))).ToList();
                Console.WriteLine();
                Console.ForegroundColor = _defaultForegroundColor;

                Console.WriteLine("Directories to be deleted:");
                Console.WriteLine();
                Console.ForegroundColor = _directoryColor;
                directories.ForEach(p => Console.WriteLine(p));
                Console.WriteLine();
                Console.ForegroundColor = _defaultForegroundColor;

                Console.WriteLine("Files to be deleted:");
                Console.WriteLine();
                Console.ForegroundColor = _fileColor;
                files.ForEach(p => Console.WriteLine(p));
                Console.WriteLine();
                Console.ForegroundColor = _defaultForegroundColor;

                if (directories.Count == 0 && files.Count == 0)
                {
                    Console.WriteLine("No files or directories to be deleted.");
                    Console.WriteLine("Press any key to continue ...");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Delete directories and files? (Y/N)  ");
                string response = Console.ReadLine();
                if (response.ToLower() != "y")
                {
                    Console.WriteLine("Operation canceled.");
                    Console.WriteLine("Press any key to continue ...");
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine();
                directories.ForEach(p => DeleteDirectoryRecursive(new DirectoryInfo(p)));
                files.ForEach(p => DeleteFileForce(new FileInfo(p)));
                Console.ForegroundColor = _defaultForegroundColor;
                Console.WriteLine();
                Console.WriteLine("Successfully deleted all files.");
                Console.WriteLine("Press any key to continue ...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        public static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
        }

        private static string GetExecutingDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6);
        }

        private static List<string> GetDirectoryFilters()
        {
            return new List<string>()
            {
                "bin",
                "obj",
                "Debug",
                "Release",
            };
        }

        private static List<string> GetFileFilters()
        {
            return new List<string>()
            {
                ".user"
            };
        }

        /// <summary>
        /// Deletes a directory recursively: all sub directory and all files within it.
        /// If a file within the directory or sub-directory is marked as read-only it will be edit it as not read-only and then deleted.
        /// </summary>
        public static void DeleteDirectoryRecursive(DirectoryInfo baseDirectoryPath)
        {
            if (!baseDirectoryPath.Exists)
            {
                return;
            }
            foreach (DirectoryInfo dir in baseDirectoryPath.EnumerateDirectories())
            {
                DeleteDirectoryRecursive(dir);
            }
            var files = baseDirectoryPath.GetFiles();
            foreach (var file in files)
            {
                DeleteFileForce(file);
            }
            Console.ForegroundColor = _deleteColor;
            Console.Write(string.Format("Deleting Directory: "));
            Console.ForegroundColor = _directoryColor;
            Console.WriteLine(baseDirectoryPath.FullName);
            baseDirectoryPath.Delete();
        }

        /// <summary>
        /// Deletes a file and if it's set to read-only it will edit it as not read-only and then deleted.
        /// </summary>
        public static void DeleteFileForce(FileInfo file)
        {
            if (!file.Exists)
            {
                return;
            }
            if (file.IsReadOnly)
            {
                file.IsReadOnly = false;
            }
            Console.ForegroundColor = _deleteColor;
            Console.Write(string.Format("Deleting File: "));
            Console.ForegroundColor = _fileColor;
            Console.WriteLine(file.FullName);
            file.Delete();
        }

        #endregion //Methods
    }
}