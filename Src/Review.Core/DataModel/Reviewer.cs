#region using

using System.ComponentModel;
using System.Runtime.Serialization;

#endregion

namespace Review.Core.DataModel
{
    [DataContract]
    public class Reviewer
    {
        public Reviewer()
        {
            CanApprovePullRequest = true;
        }

        /// <summary>
        ///     Unique id of the reviewer
        /// </summary>
        [DataMember(Order = 1)]
        public string Id { get; set; }

        /// <summary>
        ///     Reviewers display name
        /// </summary>
        [DataMember(Order = 2)]
        public string Name { get; set; }

        /// <summary>
        ///     Represents availability of this reviewer for reviews.
        /// </summary>
        [DataMember(Order = 3)]
        public ReviewerStatus Status { get; set; }

        /// <summary>
        ///     Number of reviews this reviewer did
        /// </summary>
        [DataMember(Order = 4)]
        public int ReviewCount { get; set; }

        /// <summary>
        ///     Represents a review debt among all the reviewers. It can be understood as how many reviews he owes to other
        ///     reviewers.
        /// </summary>
        [DataMember(Order = 5)]
        public int ReviewDebt { get; set; }

        /// <summary>
        ///     A flag to split reviewers to two levels. If this flag is set to true it means that review by this reviewer is
        ///     enough
        ///     to approve pull request. If this flag is false than a review from someone with this flag is needed afterwards.
        ///     Good for new team members that are learning the code base, they will do the first review and then later someone
        ///     more
        ///     experienced with this flag set will do 2nd review to approve the pull request.
        /// </summary>
        [DataMember(Order = 6)]
        [DefaultValue(true)]
        public bool CanApprovePullRequest { get; set; }

        public bool IsAvailable => Status == ReviewerStatus.Available;

        public bool IsBusy => Status == ReviewerStatus.Busy;

        public bool IsSuspended => Status == ReviewerStatus.Suspended;

        public override string ToString()
        {
            return $"{Name} ({Status}): {nameof(ReviewCount)}: {ReviewCount}, {nameof(ReviewDebt)}: {ReviewDebt}";
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