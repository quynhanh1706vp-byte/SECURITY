using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Email
{
    public class MailTemplateModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public int Type { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Variables { get; set; }
        // public string HtmlPreview { get; set; }
        public bool IsEnable { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string DetailVariables { get; set; }
    }

    public class MailTemplateTypeModel
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        // public bool BlockSendMail { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}