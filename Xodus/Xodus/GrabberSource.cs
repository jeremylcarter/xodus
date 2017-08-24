using System.Collections.Generic;

namespace Xodus
{
    public class Datum
    {
        public string label { get; set; }
        public string file { get; set; }
        public string type { get; set; }
        public bool? @default { get; set; }
    }

    public class GrabberSource
    {
        public string token { get; set; }
        public object error { get; set; }
        public List<Datum> data { get; set; }
    }
}