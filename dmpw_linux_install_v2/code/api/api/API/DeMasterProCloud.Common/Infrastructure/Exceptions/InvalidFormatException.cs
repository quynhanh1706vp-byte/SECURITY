using System;

namespace DeMasterProCloud.Common.Infrastructure.Exceptions
{
    /// <inheritdoc />
    public class InvalidFormatException : Exception
    {
        public InvalidFormatException()
        { }

        /// <inheritdoc />
        /// <summary>
        /// Constructor for Class
        /// </summary>
        public InvalidFormatException(string message)
            : base(message)
        { }

        /// <inheritdoc />
        /// <summary>
        /// Constructor for Class
        /// </summary>
        public InvalidFormatException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
