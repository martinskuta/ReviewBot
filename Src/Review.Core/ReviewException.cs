using System;

namespace Review.Core
{
    /// <summary>
    ///     Base exception class for all exceptions thrown by the review framework.
    /// </summary>
    public abstract class ReviewException : Exception
    {
        protected ReviewException(string message) : base(message)
        {
        }
    }
}