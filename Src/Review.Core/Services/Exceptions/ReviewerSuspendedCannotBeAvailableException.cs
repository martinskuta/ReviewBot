namespace Review.Core.Services.Exceptions
{
    public class ReviewerSuspendedCannotBeAvailableException : ReviewException
    {
        public ReviewerSuspendedCannotBeAvailableException(string reviewerId) : base(
            $"Reviewer '{reviewerId}' cannot be available, because he is suspended. Resume reviewer first.")
        {
        }
    }
}