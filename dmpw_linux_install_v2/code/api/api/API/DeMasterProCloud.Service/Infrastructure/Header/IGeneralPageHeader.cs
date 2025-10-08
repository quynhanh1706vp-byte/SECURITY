using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.Service.Infrastructure.Header
{
    public interface IGeneralPageHeader
    {
        List<HeaderData> GetHeaderList(PlugIn plugIn);
    }

    public class GeneralPageHeader : IGeneralPageHeader
    {
        public List<HeaderData> GetHeaderList(PlugIn plugIn)
        {
            return new List<HeaderData>();
        }
    }
}
