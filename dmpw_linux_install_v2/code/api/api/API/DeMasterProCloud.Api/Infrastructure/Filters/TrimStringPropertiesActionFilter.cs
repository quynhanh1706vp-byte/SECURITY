using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections;
using System.Reflection;

namespace DeMasterProCloud.Api.Infrastructure.Filters
{
    /// <summary>
    /// Action filter to automatically trim string properties on all incoming models
    /// </summary>
    public class TrimStringPropertiesActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument != null)
                {
                    TrimStringProperties(argument);
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }

        private void TrimStringProperties(object obj)
        {
            if (obj == null) return;

            var objType = obj.GetType();

            // Skip primitive types and strings (already handled)
            if (objType.IsPrimitive || objType == typeof(string) || objType == typeof(DateTime) || objType == typeof(DateTime?))
                return;

            // Handle collections
            if (obj is IEnumerable enumerable && !(obj is string))
            {
                foreach (var item in enumerable)
                {
                    TrimStringProperties(item);
                }
                return;
            }

            // Get all properties
            var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                var propertyValue = property.GetValue(obj);
                if (propertyValue == null)
                    continue;

                // Trim string properties
                if (property.PropertyType == typeof(string))
                {
                    var stringValue = (string)propertyValue;
                    if (stringValue != null)
                    {
                        property.SetValue(obj, stringValue.Trim());
                    }
                }
                // Recursively handle complex objects
                else if (!property.PropertyType.IsPrimitive &&
                         property.PropertyType != typeof(DateTime) &&
                         property.PropertyType != typeof(DateTime?) &&
                         property.PropertyType != typeof(decimal) &&
                         property.PropertyType != typeof(decimal?) &&
                         !property.PropertyType.IsEnum)
                {
                    TrimStringProperties(propertyValue);
                }
            }
        }
    }
}