using System.Collections.Generic;

namespace DeMasterProCloud.DataModel
{
    public class Link
    {
        public string Text { get; set; }
        public string Tilte { get; set; }   
        public string Url { get; set; }
        public string Target { get; set; }
        public string FontIcon { get; set; }
        public string ImageIcon { get; set; }
        public bool IsAcive { get; set; }
        public IList<Link> Links { get; set; }
    }
}
