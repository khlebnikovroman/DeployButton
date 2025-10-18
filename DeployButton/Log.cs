namespace DeployButton
{
    public static class Log
    {
        public static void Error(string message)
        {
            System.Diagnostics.Trace.TraceError(message);
        }
        public static void Warning(string message)
        {
            System.Diagnostics.Trace.TraceWarning(message);
        }
        public static void Info(string message)
        {
            System.Diagnostics.Trace.TraceInformation(message);
        }
    }
}