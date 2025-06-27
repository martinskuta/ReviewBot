namespace Review.Core.Services.Exceptions;

public class ReviewerAlreadyBusyException : ReviewException
{
    public ReviewerAlreadyBusyException(string reviewerId)
        : base($"Reviewer '{reviewerId}' is already busy.")
    {
    }
}