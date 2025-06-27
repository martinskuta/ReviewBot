namespace Review.Core.Services.Exceptions;

public class ReviewerAlreadySuspendedException : ReviewException
{
    public ReviewerAlreadySuspendedException(string reviewerId) : base(
        $"Reviewer '{reviewerId}' is already suspended.")
    {
    }
}