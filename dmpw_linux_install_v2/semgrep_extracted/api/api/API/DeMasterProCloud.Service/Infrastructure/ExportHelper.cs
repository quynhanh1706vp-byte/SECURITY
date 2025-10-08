using ClosedXML.Excel;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.Category;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Service.Infrastructure.Header;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.Service.Infrastructure
{
    public class ExportHelper
    {
        private readonly IConfiguration _configuration;

        private readonly int _companyId;
        private readonly int _accountId;

        public ExportHelper(IConfiguration configuration, int companyId, int accountId)
        {
            if (configuration == null)
                _configuration = ApplicationVariables.Configuration;
            else
                _configuration = configuration;

            _companyId = companyId;
            _accountId = accountId;
        }

        public ExportResult ExportDataToFile<T>(List<T> data, string pageName, string type)
        {
            string extension;
            string fileMIME;
            byte[] fileByte;

            switch (type.ToLower())
            {
                case Constants.Hancell:
                    extension = "cell";
                    fileMIME = "application/octet-stream";
                    fileByte = ExportDataToHancell(data, pageName);
                    break;
                case Constants.CSV:
                    extension = "csv";
                    fileMIME = "text/csv";
                    fileByte = ExportDataToCsv(data, pageName);
                    break;
                case Constants.Excel:
                default:
                    extension = "xlsx";
                    fileMIME = "application/ms-excel";
                    fileByte = ExportDataToExcel(data, pageName);
                    break;
            }

            ExportResult result = new()
            {
                Extension = extension,
                FileMIME = fileMIME,
                FileByte = fileByte
            };

            return result;
        }

        public byte[] ExportDataToExcel<T>(List<T> data, string pageName)
        {
            List<HeaderData> header = [];
            if (!string.IsNullOrEmpty(pageName))
            {
                PageHeader pageHeader = new PageHeader(_configuration, pageName, _companyId);
                header = pageHeader.GetHeaderList(_companyId, _accountId);
            }
            header = header.Where(h => h.IsVisible && h.HeaderVariable.ToLower() != "id" && h.IsVisible && h.HeaderVariable.ToLower() != "action" && !string.IsNullOrWhiteSpace(h.HeaderName)).OrderBy(h => h.HeaderOrder).ToList();

            byte[] result;

            using (var package = new ExcelPackage())
            {
                // add a new worksheet to the empty workbook
                var worksheet = package.Workbook.Worksheets.Add(pageName); //Worksheet name

                for (var i = 0; i < header.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = header[i].HeaderName;
                }

                worksheet.View.FreezePanes(2, 1);

                var recordIndex = 2;
                foreach (var item in data)
                {
                    var colIndex = 1;

                    foreach (var headerData in header)
                    {
                        if (!headerData.IsCategory)
                        {
                            PropertyDescriptor prop2 = TypeDescriptor.GetProperties(typeof(T)).Find(headerData.HeaderVariable, true);
                            if (prop2 == null) continue;

                            var value = prop2.GetValue(item);

                            if (prop2.PropertyType == typeof(List<CardModel>))
                                worksheet.Cells[recordIndex, colIndex++].Value = value != null ? string.Join(", ", (value as List<CardModel>).Select(d => d.CardId)) : "";
                            else
                            {
                                if (headerData.HeaderVariable.Equals("message", StringComparison.OrdinalIgnoreCase)
                                || headerData.HeaderVariable.Equals("details", StringComparison.OrdinalIgnoreCase))
                                {
                                    string strValue = (string)value;
                                    if (!string.IsNullOrWhiteSpace(strValue))
                                    {
                                        strValue = strValue.Replace("<br />", "\n");
                                        strValue = strValue.Replace("\n", Environment.NewLine);

                                        worksheet.Cells[recordIndex, colIndex++].Value = strValue;
                                        worksheet.Row(recordIndex).Height = Math.Max(20, strValue.Split('\n').Length * 15);
                                    }
                                }
                                else
                                    worksheet.Cells[recordIndex, colIndex++].Value = value;
                            }
                        }
                        else
                        {
                            var categoryOptionStr = JObject.Parse(JsonConvert.SerializeObject(item)).SelectToken("CategoryOptions").ToString();
                            var categoryOptions = JsonConvert.DeserializeObject<List<UserCategoryDataModel>>(categoryOptionStr);
                            if (categoryOptions == null || categoryOptions.Count == 0) continue;

                            if (Int32.TryParse(headerData.HeaderVariable, out int categoryId))
                            {
                                var value = categoryOptions.FirstOrDefault(co => co.Category.Id == categoryId)?.OptionName;
                                worksheet.Cells[recordIndex, colIndex++].Value = value;
                            }
                        }
                    }

                    recordIndex++;
                }

                worksheet.Cells.AutoFitColumns();
                package.Workbook.Styles.UpdateXml();
                result = package.GetAsByteArray();
            }
            return result;
        }

        public byte[] ExportDataToCsv<T>(List<T> data, string pageName)
        {
            List<HeaderData> header = [];
            if (!string.IsNullOrEmpty(pageName))
            {
                PageHeader pageHeader = new PageHeader(_configuration, pageName, _companyId);
                header = pageHeader.GetHeaderList(_companyId, _accountId);
            }
            header = header.Where(h => h.IsVisible && h.HeaderVariable.ToLower() != "id" && h.IsVisible && h.HeaderVariable.ToLower() != "action" && !string.IsNullOrWhiteSpace(h.HeaderName)).OrderBy(h => h.HeaderOrder).ToList();

            List<object[]> dataList = [];

            foreach (var item in data)
            {
                var colIndex = 0;
                object[] dataItem = new object[header.Count];

                foreach (var headerData in header)
                {
                    if (!headerData.IsCategory)
                    {
                        PropertyDescriptor prop2 = TypeDescriptor.GetProperties(typeof(T)).Find(headerData.HeaderVariable, true);
                        if (prop2 == null) continue;

                        var value = prop2.GetValue(item);
                        if (prop2.PropertyType == typeof(List<CardModel>))
                            dataItem[colIndex++] = value != null ? (value as List<CardModel>).Select(d => d.CardId).FirstOrDefault() : "";
                        else
                        {
                            if (headerData.HeaderVariable.Equals("message", StringComparison.OrdinalIgnoreCase)
                                || headerData.HeaderVariable.Equals("details", StringComparison.OrdinalIgnoreCase))
                            {
                                string strValue = (string)value;
                                if (!string.IsNullOrWhiteSpace(strValue))
                                {
                                    strValue = strValue.Replace("<br />", "\n");
                                    strValue = strValue.Replace("\n", " | ");
                                    dataItem[colIndex++] = strValue;
                                }
                            }
                            else
                                dataItem[colIndex++] = value;
                        }
                    }
                    else
                    {
                        var categoryOptionStr = JObject.Parse(JsonConvert.SerializeObject(item)).SelectToken("CategoryOptions").ToString();
                        var categoryOptions = JsonConvert.DeserializeObject<List<UserCategoryDataModel>>(categoryOptionStr);
                        if (categoryOptions == null || categoryOptions.Count == 0) continue;

                        if (Int32.TryParse(headerData.HeaderVariable, out int categoryId))
                        {
                            var value = categoryOptions.FirstOrDefault(co => co.Category.Id == categoryId)?.OptionName;
                            dataItem[colIndex++] = value;
                        }
                    }
                }

                dataList.Add(dataItem);
            }

            var sb = new StringBuilder();
            dataList.ForEach(line => { sb.AppendLine(string.Join(",", line)); });

            byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", header.Select(h => h.HeaderName))}\r\n{sb}");
            buffer = [.. Encoding.UTF8.GetPreamble(), .. buffer];

            return buffer;
        }


        public byte[] ExportDataToHancell<T>(List<T> data, string pageName)
        {
            List<HeaderData> header = [];
            if (!string.IsNullOrEmpty(pageName))
            {
                PageHeader pageHeader = new PageHeader(_configuration, pageName, _companyId);
                header = pageHeader.GetHeaderList(_companyId, _accountId);
            }
            header = header.Where(h => h.IsVisible && h.HeaderVariable.ToLower() != "id" && h.IsVisible && h.HeaderVariable.ToLower() != "action" && !string.IsNullOrWhiteSpace(h.HeaderName)).OrderBy(h => h.HeaderOrder).ToList();

            using (XLWorkbook package = new XLWorkbook())
            {
                // add a new worksheet to the empty workbook
                var worksheet = package.Worksheets.Add(pageName); //Worksheet name

                for (var i = 0; i < header.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = header[i].HeaderName;
                }

                worksheet.SheetView.Freeze(2, 1);

                var recordIndex = 2;
                foreach (var item in data)
                {
                    var colIndex = 1;

                    foreach (var headerData in header)
                    {
                        if (!headerData.IsCategory)
                        {
                            PropertyDescriptor prop2 = TypeDescriptor.GetProperties(typeof(T)).Find(headerData.HeaderVariable, true);
                            if (prop2 == null) continue;

                            var value = prop2.GetValue(item);
                            if (prop2.PropertyType == typeof(List<CardModel>))
                                worksheet.Cell(recordIndex, colIndex++).Value = value != null ? string.Join(", ", (value as List<CardModel>).Select(d => d.CardId)) : "";
                            else
                                if (headerData.HeaderVariable.Equals("message", StringComparison.OrdinalIgnoreCase)
                                || headerData.HeaderVariable.Equals("details", StringComparison.OrdinalIgnoreCase))
                            {
                                string strValue = (string)value;
                                if (!string.IsNullOrWhiteSpace(strValue))
                                {
                                    strValue = strValue.Replace("<br />", "\n");
                                    strValue = strValue.Replace("\n", Environment.NewLine);

                                    worksheet.Cell(recordIndex, colIndex++).Value = strValue;
                                    worksheet.Row(recordIndex).Height = Math.Max(20, strValue.Split('\n').Length * 15);
                                }
                            }
                            else
                                worksheet.Cell(recordIndex, colIndex++).Value = (XLCellValue)value;
                        }
                        else
                        {
                            var categoryOptionStr = JObject.Parse(JsonConvert.SerializeObject(item)).SelectToken("CategoryOptions").ToString();
                            var categoryOptions = JsonConvert.DeserializeObject<List<UserCategoryDataModel>>(categoryOptionStr);
                            if (categoryOptions == null || categoryOptions.Count == 0) continue;

                            if (Int32.TryParse(headerData.HeaderVariable, out int categoryId))
                            {
                                var value = categoryOptions.FirstOrDefault(co => co.Category.Id == categoryId)?.OptionName;
                                worksheet.Cell(recordIndex, colIndex++).Value = (XLCellValue)value;
                            }
                        }
                    }

                    recordIndex++;
                }

                using (MemoryStream fs = new MemoryStream())
                {
                    package.SaveAs(fs);
                    fs.Position = 0;
                    return fs.ToArray();
                }
            }
        }
    }


    public class ExportResult
    {
        public string Extension { get; set; }

        public string FileMIME { get; set; }

        public byte[] FileByte { get; set; }
    }
}
