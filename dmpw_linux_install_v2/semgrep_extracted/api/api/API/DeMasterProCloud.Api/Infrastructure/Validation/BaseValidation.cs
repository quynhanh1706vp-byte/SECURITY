using DeMasterProCloud.Common.Infrastructure;
using FluentValidation;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Base validation class containing common validation methods
    /// </summary>
    /// <typeparam name="T">The model type to validate</typeparam>
    public abstract class BaseValidation<T> : AbstractValidator<T>
    {
        /// <summary>
        /// Checks if a string contains only safe characters using the Helpers class
        /// </summary>
        /// <param name="input">The string to validate</param>
        /// <returns>True if the string contains only safe characters</returns>
        protected bool ContainsOnlySafeCharacters(string input)
        {
            return Helpers.ContainsOnlyLettersAndDigits(input) && !Helpers.ContainsCode(input);
        }
        /// <summary>
        /// Checks if a string contains only safe characters using the Helpers class
        /// </summary>
        /// <param name="input">The string to validate</param>
        /// <returns>True if the string contains only safe characters</returns>
        protected bool NotContainsCode(string input)
        {
            return !Helpers.ContainsCode(input);
        }
    }
}