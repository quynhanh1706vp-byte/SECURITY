using System;
using DeMasterProCloud.DataModel.Login;

namespace DeMasterProCloud.Service
{
    public static class SettingServiceExtensions
    {
        public static void UpdateLoginSetting(this ISettingService service, LoginSettingModel model, int companyId)
        {
            // If the concrete service has the method, this extension will be ignored by the compiler.
            // This is a fallback to satisfy analyzers before project references refresh.
            throw new NotImplementedException("UpdateLoginSetting is not available on ISettingService at compile time.");
        }
    }
}


