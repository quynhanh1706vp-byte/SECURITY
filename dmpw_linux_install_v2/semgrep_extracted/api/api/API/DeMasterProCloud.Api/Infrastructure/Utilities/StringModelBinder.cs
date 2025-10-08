using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace DeMasterProCloud.Api.Infrastructure.Utilities
{
    /// <summary>
    /// A custom ModelBinder for strings
    /// </summary>
    public class StringModelBinder : IModelBinder
    {
        /// <summary>
        /// Bind model async
        /// </summary>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelName = bindingContext.ModelName;
            if (string.IsNullOrEmpty(modelName))
                return Task.CompletedTask;

            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            var value = !string.IsNullOrEmpty(valueProviderResult.FirstValue)
                ? valueProviderResult.FirstValue.Trim()
                : null;
            bindingContext.Result = ModelBindingResult.Success(value);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// A custom ModelBinderProvider that references the custom ModelBinder
    /// </summary>
    public class ModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// Get binder
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context != null)
            {
                if (context.Metadata.ModelType == typeof(string))
                {
                    return new BinderTypeModelBinder(typeof(StringModelBinder));
                }
            }
            return null;
        }
    }
}
