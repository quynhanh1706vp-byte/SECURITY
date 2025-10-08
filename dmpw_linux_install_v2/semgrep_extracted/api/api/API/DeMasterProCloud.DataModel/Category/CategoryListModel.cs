using DeMasterProCloud.DataAccess.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeMasterProCloud.DataModel.Category
{
    public class CategoryListModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ParentCategoryId { get; set; }

        public List<int> OptionList { get; set; }

    }
    public class CategoryPagingData<T1, T2>
    {
        public CategoryPagingData()
        {
            Meta = new Meta();
        }

        public List<T1> Categories { get; set; }
        public List<T2> Options { get; set; }
        public Meta Meta { get; set; }
    }

    public class CategoryData<T1, T2>
    {
        public CategoryData()
        {

        }

        public T1 Category { get; set; }
        public List<T2> Options { get; set; }
    }

    public class CategoryHeaderModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class UserCategoryDataModel
    {
        public UserCategoryDataModel()
        {
            Category = new CategoryHeaderModel();
        }

        public CategoryHeaderModel Category { get; set; }

        public string OptionName { get; set; }
    }
}