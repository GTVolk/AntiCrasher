using System;
using System.Runtime.InteropServices;

namespace AntiCrasher
{
    class Program
    {

        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler eventHandler;
        static AntiCrasher antiCrasher;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            antiCrasher.cleanProcess();

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }
        #endregion

        public void Start()
        {
            // start a thread and start doing some processing
            Console.WriteLine("Thread started, processing..");
        }

        static int Main(string[] args)
        {
            // Create class instance
            antiCrasher = new AntiCrasher();

            // Some boilerplate to react to close window event, CTRL-C, kill, etc
            eventHandler += new EventHandler(Handler);
            SetConsoleCtrlHandler(eventHandler, true);

            // Parsing arguments
            int errNo = antiCrasher.parseCommandLineArguments(args);
            // Check success
            if (errNo != AntiCrasher.RESULT_SUCCESS)
            {
                Console.WriteLine(
                    "Usage error!\n"
                    + "Valid usage: anticrasher.exe [-title Window title] [-target Target description] [-hidden] executable [params]\n\n"
                    + ""
                    + "Command reference:\n\n"
                    + ""
                    + "-title = Window title\n"
                    + "-target = If specified, then shows as protecting target in main window, otherwise executable path is shown\n"
                    + "-hidden = Should to hide executable window\n"
                    + "-always = Should to restart app even if exit code 0\n"
                    + "executable = Main protecting executable\n"
                    + "params = Executable file params\n"
                    + ""
                );
                return errNo;
            }
            // Check and set title
            if (!String.IsNullOrEmpty(antiCrasher.WindowTitle))
            {
                Console.Title = "Anti-crash " + antiCrasher.WindowTitle;
            }
            else
            {
                Console.Title = "Anti-crasher";
            }
            // Check and set target
            if (!String.IsNullOrEmpty(antiCrasher.TargetName))
            {
                Console.WriteLine("Protecting " + antiCrasher.TargetName + " from crashes...");
            }
            else
            {
                Console.WriteLine("Protecting " + antiCrasher.ExecutablePath + " from crashes...");
            }
            // Anticrash process
            return antiCrasher.runProcess(true);
        }
    }
}