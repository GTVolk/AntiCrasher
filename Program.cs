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
        static EventHandler _handler;
        static AntiCrasher _ac;

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

            _ac.cleanProcess();

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
            _ac = new AntiCrasher();

            // Some boilerplate to react to close window event, CTRL-C, kill, etc
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            // Parsing arguments
            int errNo = _ac.parseArguments(args);
            // Check success
            if (errNo != AntiCrasher.SUCCESS)
            {
                Console.WriteLine("Usage error!\nValid usage: anticrasher.exe [-title Window title] [-target Target description] protecting_file [protecting_file_params]");
                return errNo;
            }
            // Check and set title
            if (!String.IsNullOrEmpty(_ac.Title))
            {
                Console.Title = "Anti-crash " + _ac.Title;
            }
            else
            {
                Console.Title = "Anti-crasher";
            }
            // Check and set target
            if (!String.IsNullOrEmpty(_ac.Target))
            {
                Console.WriteLine("Protecting " + _ac.Target + " from crashes...");
            }
            else
            {
                Console.WriteLine("Protecting " + _ac.ExecFile + " from crashes...");
            }
            // Anticrash process
            return _ac.runProcess(true);
        }
    }
}