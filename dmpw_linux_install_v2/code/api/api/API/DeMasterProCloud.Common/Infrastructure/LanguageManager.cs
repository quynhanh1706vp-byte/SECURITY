using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using DeMasterProCloud.Common.Resources;

namespace DeMasterProCloud.Common.Infrastructure
{
    public class LanguageManager : FluentValidation.Resources.LanguageManager
    {
        public LanguageManager(IOptions<RequestLocalizationOptions> locOptions)
        {
            var cultures = locOptions.Value.SupportedUICultures.Select(m => m.Name).ToList();
            foreach (var culture in cultures)
            {
                AddTranslation(culture, "NotNullValidator",
                    MessageResource.ResourceManager.GetString("NotNullValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "NotEmptyValidator",
                    MessageResource.ResourceManager.GetString("NotEmptyValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "NotEqualValidator",
                    MessageResource.ResourceManager.GetString("NotEqualValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "EqualValidator",
                    MessageResource.ResourceManager.GetString("EqualValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "LengthValidator",
                    MessageResource.ResourceManager.GetString("LengthValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "MaximumLengthValidator",
                    MessageResource.ResourceManager.GetString("MaximumLengthValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "MinimumLengthValidator",
                    MessageResource.ResourceManager.GetString("MinimumLengthValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "LessThanValidator",
                    MessageResource.ResourceManager.GetString("LessThanValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "LessThanOrEqualValidator",
                    MessageResource.ResourceManager.GetString("LessThanOrEqualValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "GreaterThanValidator",
                    MessageResource.ResourceManager.GetString("GreaterThanValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "GreaterThanOrEqualValidator",
                    MessageResource.ResourceManager.GetString("GreaterThanOrEqualValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "RegularExpressionValidator",
                    MessageResource.ResourceManager.GetString("RegularExpressionValidator",
                        new CultureInfo(culture)));
                AddTranslation(culture, "InclusiveBetweenValidator",
                    MessageResource.ResourceManager.GetString("InclusiveBetweenValidator",
                        new CultureInfo(culture)));
            }
        }
    }
}