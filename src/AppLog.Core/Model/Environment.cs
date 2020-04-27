namespace AppLog.Core.Model
{
    public class Environment
    {
        public string MachineName { get; set; }
        public string[] IpAddress { get; set; }
        public string ApplicationPath { get; set; }
        public string OS { get; set; }
        public string Browser { get; set; }
    }
}