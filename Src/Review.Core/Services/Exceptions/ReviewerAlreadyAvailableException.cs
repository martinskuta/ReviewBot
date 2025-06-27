namespace Review.Core.Services.Exceptions;

public class ReviewerAlreadyAvailableException : ReviewException
{
    public ReviewerAlreadyAvailableException(string reviewerId) : base(
        $"Reviewer '{reviewerId}' is already available.")
    {
    }
}