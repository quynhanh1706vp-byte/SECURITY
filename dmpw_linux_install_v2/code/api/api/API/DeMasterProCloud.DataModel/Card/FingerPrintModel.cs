using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Card
{
    public class FingerPrintModel
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public List<string> Templates { get; set; }
    }
}