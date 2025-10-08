using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Setting
{
    public class QrCodeStoredOutsideService
    {
        public string Email { get; set; }
        public string Qr { get; set; }
        public int Duration { get; set; }
    }
    
    #region Mapping data in 4T

    public class ImportCheckInForCompany
    {
        public string CompanyCode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string MethodAuth { get; set; }
    }

    public class BodyConfigReportImport{
        public ConfigReportImportDTO ConfigReportImportDTO { get; set; }
        public ConfigReportDataImport ConfigReportDataImport { get; set; }
    }
    
    public class ConfigReportImportDTO
    {
        public int ReportId { get; set; }
        public string ImportTime { get; set; }
    }

    public class ConfigReportDataImport
    {
        public List<object> MapData { get; set; }
    }

    public class ConfigMapDataSo_luong
    {
        public double So_luong { get; set; }
        public string Thoi_gian { get; set; }
    }

    public class ConfigMapDataGia_tri
    {
        public double Gia_tri { get; set; }
        public double So_luong { get; set; }
        public string Thoi_gian { get; set; }
    }

    #endregion
}