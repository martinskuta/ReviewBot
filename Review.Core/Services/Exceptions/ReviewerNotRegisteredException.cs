namespace Review.Core.Services.Exceptions
{
    public class ReviewerNotRegisteredException : ReviewException
    {
        public ReviewerNotRegisteredException(string reviewerId) : base(
            $"Reviewer with id '{reviewerId}' is not registered.")
        {
        }
    }
}