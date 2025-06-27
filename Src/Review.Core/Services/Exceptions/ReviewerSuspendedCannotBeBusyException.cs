namespace Review.Core.Services.Exceptions;

public class ReviewerSuspendedCannotBeBusyException : ReviewException
{
    public ReviewerSuspendedCannotBeBusyException(string reviewerId)
        : base(
            $"Reviewer '{reviewerId}' cannot be busy, because he is suspended. Resume reviewer first.")
    {
    }
}