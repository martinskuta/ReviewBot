using System.Runtime.Serialization;

namespace Review.Core.DataModel
{
    [DataContract]
    public class Reviewer
    {
        /// <summary>
        ///     Unique id of the reviewer
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     Represents availability of this reviewer for reviews.
        /// </summary>
        [DataMember]
        public ReviewerStatus Status { get; set; }

        /// <summary>
        ///     Number of reviews this reviewer did
        /// </summary>
        [DataMember]
        public int ReviewCount { get; set; }

        /// <summary>
        ///     Represents a review debt among all the reviewers. It can be understood as how many reviews he owes to other
        ///     reviewers.
        /// </summary>
        [DataMember]
        public int ReviewDebt { get; set; }

        public bool IsAvailable => Status == ReviewerStatus.Available;

        public bool IsBusy => Status == ReviewerStatus.Busy;

        public bool IsSuspended => Status == ReviewerStatus.Suspended;

        public override string ToString()
        {
            return $"{Id} ({Status}): {nameof(ReviewCount)}: {ReviewCount}, {nameof(ReviewDebt)}: {ReviewDebt}";
        }
    }

    public enum ReviewerStatus
    {
        /// <summary>
        ///     When reviewer is available to do reviews.
        /// </summary>
        Available,

        /// <summary>
        ///     When reviewer is busy with some work so he cannot do reviews. His debt grows when skipping review.
        /// </summary>
        Busy,

        /// <summary>
        ///     When reviewer is suspended from reviews. For example when he is on vacations. His debt doesn't grow when skipping
        ///     review.
        /// </summary>
        Suspended
    }
}