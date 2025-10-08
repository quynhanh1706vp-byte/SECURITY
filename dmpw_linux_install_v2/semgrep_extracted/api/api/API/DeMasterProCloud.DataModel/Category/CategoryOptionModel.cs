using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeMasterProCloud.DataModel.Category
{
    public class CategoryOptionModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CategoryId { get; set; }

        public int? ParentOptionId { get; set; }

    }
}