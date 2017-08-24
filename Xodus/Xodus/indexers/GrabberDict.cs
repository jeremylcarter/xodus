namespace Xodus
{
    public class Params
    {
        public string id { get; set; }
        public string token { get; set; }
        public string options { get; set; }
    }

    public class GrabberDict
    {
        public string grabber { get; set; }
        public Params @params { get; set; }
        public string target { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string subtitle { get; set; }
    }
}