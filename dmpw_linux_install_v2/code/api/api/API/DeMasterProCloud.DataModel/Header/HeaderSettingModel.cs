using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Header
{
    public class HeaderSettingModel
    {
        public string PageName { get; set; }

        public List<HeaderInfo> Headers { get; set; }
    }

    public class HeaderInfo
    {
        public int HeaderOrder { get; set; }

        public int HeaderId { get; set; }
        public string HeaderVariable { get; set; }
        public bool IsVisible { get; set; }
    }
}
