using System.Collections.Generic;
using DeMasterProCloud.DataAccess.Models;

namespace DeMasterProCloud.Service
{
    public class BaseReportModel<T>
    {
        public BaseReportModel(Company company)
        {
            Rows = new List<T>();
            Code = company.Code;
            Name = company.Name;
            Contact = company.Contact;
            Logo = GetCompanyLogo(company);
        }
        public List<T> Rows { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Logo { get; set; }

        private string GetCompanyLogo(Company company)
        {
            if (company.Logo != null)
            {
                return System.Text.Encoding.UTF8.GetString(company.Logo);
            }
            return company.MiniLogo != null ? System.Text.Encoding.UTF8.GetString(company.MiniLogo) : string.Empty;
        }
    }
}
