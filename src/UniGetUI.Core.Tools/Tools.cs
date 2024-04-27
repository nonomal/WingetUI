﻿using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using UniGetUI.Core.Data;
using UniGetUI.Core.Language;
using UniGetUI.Core.Logging;

namespace UniGetUI.Core.Tools
{
    public class CoreTools
    {

        private static LanguageEngine LanguageEngine;
        private static CoreTools? instance = null;

        private static CoreTools Instance
        {
            get
            {
                if (instance == null) instance = new CoreTools();
                return instance;
            }
        }
        private CoreTools()
        {
            LanguageEngine = new LanguageEngine();
        }

        /// <summary>
        /// Generates a random string composed of characters in a-z and digits in 0-9 
        /// </summary>
        /// <param name="length">The desired length of the string</param>
        /// <returns>A random string</returns>
        public string GetRandomString(int length)
        {
            Random random = new();
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            IEnumerable<char> chars = Enumerable.Range(0, length)
                .Select(x => pool[random.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }

        /// <summary>
        /// Translate a string to the current language
        /// </summary>
        /// <param name="text">The string to translate</param>
        /// <returns>The translated string if available, the original string otherwise</returns>
        public static string Translate(string text)
        {
            if(LanguageEngine == null) LanguageEngine = new LanguageEngine();
            return LanguageEngine.Translate(text);
        }

        /// <summary>
        /// Dummy function to capture the strings that need to be translated but the translation is handled by a custom widget
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AutoTranslated(string text)
        {
            return text;
        }

        /// <summary>
        /// Launches the self executable on a new process and kills the current process
        /// </summary>
        public static void RelaunchProcess()
        {
            Logger.Log(Environment.GetCommandLineArgs()[0].Replace(".dll", ".exe"));
            System.Diagnostics.Process.Start(Environment.GetCommandLineArgs()[0].Replace(".dll", ".exe"));
            Environment.Exit(0);
        }

        /// <summary>
        /// Finds an executable in path and returns its location
        /// </summary>
        /// <param name="command">The executable alias to find</param>
        /// <returns>A tuple containing: a boolean hat represents wether the path was found or not; the path to the file if found.</returns>
        public static async Task<Tuple<bool, string>> Which(string command)
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = "/C where " + command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string line = await process.StandardOutput.ReadLineAsync();
            string output;
            if (line == null)
                output = "";
            else
                output = line.Trim();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0 || output == "")
                return new Tuple<bool, string>(false, "");
            else
                return new Tuple<bool, string>(File.Exists(output), output);
        }

        /// <summary>
        /// Formats a given package id as a name, capitalizing words and replacing separators with spaces
        /// </summary>
        /// <param name="name">A string containing the Id of a package</param>
        /// <returns>The formatted string</returns>
        public static string FormatAsName(string name)
        {
            name = name.Replace(".install", "").Replace(".portable", "").Replace("-", " ").Replace("_", " ").Split("/")[^1];
            string newName = "";
            for (int i = 0; i < name.Length; i++)
            {
                if (i == 0 || name[i - 1] == ' ')
                    newName += name[i].ToString().ToUpper();
                else
                    newName += name[i];
            }
            return newName;
        }

        /// <summary>
        /// Generates a random string composed of alphanumeric characters and numbers
        /// </summary>
        /// <param name="length">The length of the string</param>
        /// <returns>A string</returns>
        public static string RandomString(int length)
        {
            Random random = new();
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            IEnumerable<char> chars = Enumerable.Range(0, length)
                .Select(x => pool[random.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }

        public static void Log(Exception e)
        { Log(e.ToString()); }

        public static void Log(object o)
        { if (o != null) Log(o.ToString()); else Log("null"); }

        public static void ReportFatalException(Exception e)
        {
            string LangName = "Unknown";
            try
            {
                LangName = LanguageEngine.MainLangDict["langName"];
            }
            catch { }

            string Error_String = $@"
                        OS: {Environment.OSVersion.Platform}
                   Version: {Environment.OSVersion.VersionString}
           OS Architecture: {Environment.Is64BitOperatingSystem}
          APP Architecture: {Environment.Is64BitProcess}
                  Language: {LangName}
               APP Version: {CoreData.VersionName}
                Executable: {Environment.ProcessPath}

Crash Message: {e.Message}

Crash Traceback: 
{e.StackTrace}";

            Console.WriteLine(Error_String);


            string ErrorBody = "https://www.marticliment.com/error-report/?appName=UniGetUI^&errorBody=" + Uri.EscapeDataString(Error_String.Replace("\n", "{l}"));

            Console.WriteLine(ErrorBody);

            using System.Diagnostics.Process cmd = new();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            cmd.StandardInput.WriteLine("start " + ErrorBody);
            cmd.StandardInput.WriteLine("exit");
            cmd.WaitForExit();
            Environment.Exit(1);

        }

        /// <summary>
        /// Launches a .bat or .cmd file for the given filename
        /// </summary>
        /// <param name="path">The path of the batch file</param>
        /// <param name="WindowTitle">The title of the window</param>
        /// <param name="RunAsAdmin">Whether the batch file should be launched elevated or not</param>
        public static async void LaunchBatchFile(string path, string WindowTitle = "", bool RunAsAdmin = false)
        {
            Process p = new();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/C start \"" + WindowTitle + "\" \"" + path + "\"";
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Verb = RunAsAdmin ? "runas" : "";
            p.Start();
            await p.WaitForExitAsync();
        }

        /// <summary>
        /// Checks whether the current process has administrator privileges
        /// </summary>
        /// <returns>True if the process has administrator privileges</returns>
        public static bool IsAdministrator()
        {
            try
            {
                return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                          .IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception e)
            {
                Log(e);
                return false;
            }
        }

        /// <summary>
        /// Returns the size (in MB) of the file at the given URL
        /// </summary>
        /// <param name="url">a valid Uri object containing a URL to a file</param>
        /// <returns>a double representing the size in MBs, 0 if the process fails</returns>
        public static async Task<double> GetFileSizeAsync(Uri url)
        {
            try
            {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                WebRequest req = WebRequest.Create(url);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
                req.Method = "HEAD";
                WebResponse resp = await req.GetResponseAsync();
                long ContentLength;
                if (long.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                {
                    return ContentLength / 1048576;
                }

            }
            catch (Exception e)
            {
                Log(e);
            }
            return 0;
        }
    }
}
