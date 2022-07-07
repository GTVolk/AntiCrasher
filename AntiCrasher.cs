using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AntiCrasher
{
    class AntiCrasher
    {
        #region Constants of result
        public const int RESULT_SUCCESS = 0;
        public const int RESULT_NO_ARGUMENTS = 1;
        public const int RESULT_PARAM_IS_EMPTY = 2;
        public const int RESULT_NOT_VALID_STRUCT = 3;
        public const int RESULT_NOT_VALID_FILE_PATH = 4;
        public const int RESULT_NOT_VALID_EXEC_EXT = 5;
        public const int RESULT_NO_EXIST_EXEC_FILE = 6;
        public const int RESULT_EXCEPTION_FAIL = 10;
        #endregion

        /// <summary>
        ///  Constructor
        /// </summary>
        public AntiCrasher() { }

        #region Properties

        // Private properties
        private Process Process { get; set; }

        /// <summary>
        /// Window title
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// Target name
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Is executed process window visible
        /// </summary>
        public bool IsWindowVisible { get; set; }

        /// <summary>
        /// Executable file path
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// Exec file params
        /// </summary>
        public List<string> ExecutableCommandLineParameters { get; set; }

        /// <summary>
        /// Valid exec file extensions
        /// </summary>
        public string[] ValidExecutableTypes {
            get {
                return new string[] { ".bat", ".exe", ".cmd", ".ps1", ".sh" };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parse arguments array into class variables
        /// </summary>
        /// <param name="arguments">Arguments array</param>
        /// <returns>Code of success</returns>
        public int parseCommandLineArguments(string[] arguments)
        {
            // Property initialization
            this.Process = null;
            this.WindowTitle = "";
            this.TargetName = "";
            this.IsWindowVisible = true;
            this.ExecutablePath = "";
            this.ExecutableCommandLineParameters = new List<string>();

            if (arguments.Length < 1)
                return RESULT_NO_ARGUMENTS;

            // TODO: Pattern Responsibility chain!
            for (int i = 0; i < arguments.Length; i++)
            {
                string a = arguments[i];

                if (!String.IsNullOrEmpty(a))
                {
                    if (a == "-title")
                    {
                        i++;
                        if (arguments.Length > i + 1)
                        {
                            this.WindowTitle = arguments[i];
                            continue;
                        }
                        else
                        {
                            return RESULT_NOT_VALID_STRUCT;
                        }
                    }
                    if (a == "-target")
                    {
                        i++;
                        if (arguments.Length > i + 1)
                        {
                            this.TargetName = arguments[i];
                            continue;
                        }
                        else
                        {
                            return RESULT_NOT_VALID_STRUCT;
                        }
                    }
                    if (a == "-hidden")
                    {
                        this.IsWindowVisible = false;
                        continue;
                    }
                    string ext = Path.GetExtension(a);
                    if (!String.IsNullOrEmpty(ext))
                    {
                        bool isContains = false;
                        foreach (string vExt in this.ValidExecutableTypes)
                        {
                            if (vExt == ext)
                            {
                                isContains = true;
                                break;
                            }
                        }
                        if (isContains)
                        {
                            if (File.Exists(a))
                            {
                                // Skip current exe as parameter
                                int startIndex = i + 1;
                                this.ExecutablePath = a;
                                for (int k = startIndex++; k < arguments.Length; k++)
                                {
                                    this.ExecutableCommandLineParameters.Add(arguments[k]);
                                }
                                return RESULT_SUCCESS;
                            }
                            else
                            {
                                return RESULT_NO_EXIST_EXEC_FILE;
                            }
                        }
                        else
                        {
                            return RESULT_NOT_VALID_EXEC_EXT;
                        }
                    }
                    else
                    {
                        return RESULT_NOT_VALID_FILE_PATH;
                    }
                }
                else
                {
                    return RESULT_PARAM_IS_EMPTY;
                }
            }
            return RESULT_NOT_VALID_STRUCT;
        }

        /// <summary>
        /// Runs desired process and restarting it redgarding exit code
        /// </summary>
        /// <param name="isConsole">Is method can write to console</param>
        /// <returns>Code of success</returns>
        public int runProcess(bool isConsole = false)
        {
            do
            {
                if (isConsole)
                {
                    if (!String.IsNullOrEmpty(this.TargetName))
                    {
                        Console.WriteLine(DateTime.Now + " " + this.TargetName + " is started");
                    }
                    else
                    {
                        Console.WriteLine(DateTime.Now + " " + this.ExecutablePath + " is started");
                    }
                }

                //Setup the Process with the ProcessStartInfo class
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = this.ExecutablePath;
                startInfo.Arguments = String.Join(" ", this.ExecutableCommandLineParameters.ToArray());
                if (!this.IsWindowVisible)
                {
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }

                try
                {
                    this.Process = Process.Start(startInfo);
                    this.Process.WaitForExit();
                }
                catch (Exception e)
                {
                    if (isConsole)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                    return RESULT_EXCEPTION_FAIL;
                }
                if (isConsole)
                {
                    if (this.Process.ExitCode > 0)
                    {
                        if (!String.IsNullOrEmpty(this.TargetName))
                        {
                            Console.WriteLine(DateTime.Now + " " + this.TargetName + " is closed with error code " + this.Process.ExitCode + ". Restarting...");
                        }
                        else
                        {
                            Console.WriteLine(DateTime.Now + " " + this.ExecutablePath + " is closed with error code " + this.Process.ExitCode + ". Restarting...");
                        }

                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(this.TargetName))
                        {
                            Console.WriteLine(DateTime.Now + " " + this.TargetName + " is closed. Exiting...");
                        }
                        else
                        {
                            Console.WriteLine(DateTime.Now + " " + this.ExecutablePath + " is closed. Exiting...");
                        }

                    }
                }
                Thread.Sleep(3000); // Wait 3 seconds before restart!
            }
            while (this.Process.ExitCode > 0);
            return RESULT_SUCCESS;
        }

        public void cleanProcess()
        {
            if (this.Process != null)
            {
                if (!this.Process.HasExited)
                {
                    this.Process.Kill();
                }
            }
        }

        #endregion

        /// <summary>
        /// Destructor, kills process if it still running in background
        /// </summary>
        ~AntiCrasher()
        {
            this.cleanProcess();
        }
    }
}