namespace Review.Core.Services.Exceptions
{
    public class ReviewerNotSuspendedCannotBeResumedException : ReviewException
    {
        public ReviewerNotSuspendedCannotBeResumedException(string reviewerId) : base(
            $"Reviewer '{reviewerId}' cannot be resumed, because he is not suspended.")
        {
        }
    }
}