namespace Review.Core.Services.Exceptions
{
    public class NoReviewerAvailableException : ReviewException
    {
        public NoReviewerAvailableException()
            : base("There is currently no available reviewer.")
        {
        }
    }
}