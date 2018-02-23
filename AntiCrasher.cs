using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AntiCrasher
{
    class AntiCrasher
    {
        // Constants of result
        public const int SUCCESS = 0;
        public const int NO_ARGUMENTS = 1;
        public const int PARAM_IS_EMPTY = 2;
        public const int NOT_VALID_STRUCT = 3;
        public const int NOT_VALID_FILE_PATH = 4;
        public const int NOT_VALID_EXEC_EXT = 5;
        public const int NO_EXIST_EXEC_FILE = 6;
        public const int EXCEPTION_FAIL = 10;

        // Public properties
        private string _title;
        private string _target;
        private bool _visible;
        private string _exec;
        private List<string> _params;

        // Public readonly properties
        private string[] _validExt;

        // Private properties
        private Process _p;

        /// <summary>
        ///  Constructor
        /// </summary>
        public AntiCrasher()
        {
            this._title = "";
            this._target = "";
            this._visible = true;
            this._exec = "";
            this._params = new List<string>();
            this._validExt = new string[] { ".bat", ".exe", ".cmd", ".ps1", ".sh" };
        }

        /// <summary>
        /// Window title
        /// </summary>
        public string Title {
            get { return this._title; }
            set { this._title = value; }
        }

        /// <summary>
        /// Target name
        /// </summary>
        public string Target {
            get { return this._target; }
            set { this._target = value; }
        }

        /// <summary>
        /// Is executed process window visible
        /// </summary>
        public bool isVisible {
            get { return this._visible; }
            set { this._visible = value; }
        }

        /// <summary>
        /// Exec file path
        /// </summary>
        public string ExecFile {
            get { return this._exec; }
            set { this._exec = value; }
        }

        /// <summary>
        /// Exec file params
        /// </summary>
        public List<string> ExecParams {
            get { return this._params; }
            set { this._params = value; }
        }

        /// <summary>
        /// Valid exec file extensions
        /// </summary>
        public string[] ValidExtensions {
            get { return this._validExt; }
        }

        /// <summary>
        /// Parse arguments array into class variables
        /// </summary>
        /// <param name="arguments">Arguments array</param>
        /// <returns>Code of success</returns>
        public int parseArguments(string[] arguments)
        {
            if (arguments.Length < 1)
                return NO_ARGUMENTS;

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
                            this.Title = arguments[i];
                            continue;
                        }
                        else
                        {
                            return NOT_VALID_STRUCT;
                        }
                    }
                    if (a == "-target")
                    {
                        i++;
                        if (arguments.Length > i + 1)
                        {
                            this.Target = arguments[i];
                            continue;
                        }
                        else
                        {
                            return NOT_VALID_STRUCT;
                        }
                    }
                    if (a == "-hidden")
                    {
                        this.isVisible = false;
                        continue;
                    }
                    string ext = Path.GetExtension(a);
                    if (!String.IsNullOrEmpty(ext))
                    {
                        bool isContains = false;
                        foreach (string vExt in this.ValidExtensions)
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
                                this.ExecFile = a;
                                for (int k = i++; k < arguments.Length; k++)
                                {
                                    this.ExecParams.Add(arguments[k]);
                                }
                                return SUCCESS;
                            }
                            else
                            {
                                return NO_EXIST_EXEC_FILE;
                            }
                        }
                        else
                        {
                            return NOT_VALID_EXEC_EXT;
                        }
                    }
                    else
                    {
                        return NOT_VALID_FILE_PATH;
                    }
                }
                else
                {
                    return PARAM_IS_EMPTY;
                }
            }
            return NOT_VALID_STRUCT;
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
                    if (!String.IsNullOrEmpty(this.Target))
                    {
                        Console.WriteLine(DateTime.Now + " " + this.Target + " is started");
                    }
                    else
                    {
                        Console.WriteLine(DateTime.Now + " " + this.ExecFile + " is started");
                    }
                }

                //Setup the Process with the ProcessStartInfo class
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = this.ExecFile;
                startInfo.Arguments = String.Join(" ", this.ExecParams.ToArray());
                if (!this.isVisible)
                {
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }

                try
                {
                    this._p = Process.Start(startInfo);
                    this._p.WaitForExit();
                }
                catch (Exception e)
                {
                    if (isConsole)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                    return EXCEPTION_FAIL;
                }
                if (isConsole)
                {
                    if (this._p.ExitCode > 0)
                    {
                        if (!String.IsNullOrEmpty(this.Target))
                        {
                            Console.WriteLine(DateTime.Now + " " + this.Target + " is closed with error code " + this._p.ExitCode + ". Restarting...");
                        }
                        else
                        {
                            Console.WriteLine(DateTime.Now + " " + this.ExecFile + " is closed with error code " + this._p.ExitCode + ". Restarting...");
                        }

                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(this.Target))
                        {
                            Console.WriteLine(DateTime.Now + " " + this.Target + " is closed. Exiting...");
                        }
                        else
                        {
                            Console.WriteLine(DateTime.Now + " " + this.ExecFile + " is closed. Exiting...");
                        }

                    }
                }
                Thread.Sleep(3000); // Wait 3 seconds before restart!
            }
            while (this._p.ExitCode > 0);
            return SUCCESS;
        }

        public void cleanProcess()
        {
            if (this._p != null)
            {
                if (!this._p.HasExited)
                {
                    this._p.Kill();
                }
            }
        }

        /// <summary>
        /// Destructor, kills process if it still running in background
        /// </summary>
        ~AntiCrasher()
        {
            this.cleanProcess();
        }
    }
}