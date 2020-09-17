using System.Diagnostics;
// We will use singleton pattern to create 1 single logging object.
// This will help to avoid creating many unnecessary logging instances 
// and also avoid the conflict when 2 logging instances log to the same file.
namespace OnlineBookingSystem.Models
{
    // We create MyLogger class inherits from TraceSource.
    // TraceSource will help to write information, event or error to log file.
    public class MyLogger : TraceSource
    {
        // Set instance as private so it cannot be called directly from out side of MyLogger class.
        private static MyLogger instance;
        // Below is the initializing function that follow the default format of TraceSource.
        private MyLogger(string Name) : base(Name)
        {
            this.Listeners.Clear();
            TextWriterTraceListener txt = new TextWriterTraceListener(string.Format(@"D:\log\weberrlog-{0:yyyy-MM-dd}.txt", System.DateTime.Now));
            this.Listeners.Add(txt);
            this.Switch = new SourceSwitch("mySwitch");
            this.Switch.Level = SourceLevels.All;
            Trace.AutoFlush = true;
        }
        // Use singleton pattern to get the MyLogger instance.
        public static MyLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MyLogger("Logging");
                }
                return instance;
            }
        }
    }
}