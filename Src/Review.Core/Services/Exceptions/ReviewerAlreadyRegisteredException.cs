namespace Review.Core.Services.Exceptions
{
    public class ReviewerAlreadyRegisteredException : ReviewException
    {
        public ReviewerAlreadyRegisteredException(string reviewerId)
            : base(
                $"Reviewer with id '{reviewerId}' is already registered.")
        {
        }
    }
}