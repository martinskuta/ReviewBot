using System.Collections.Generic;
using JetBrains.Annotations;
using Review.Core.DataModel;

namespace Review.Core.Services
{
    public interface IReviewService
    {
        void RegisterReviewer([NotNull] string reviewerId);

        [NotNull]
        Reviewer GetReviewer([NotNull] string reviewerId);

        List<Reviewer> GetAllReviewers();

        void SuspendReviewer([NotNull] string reviewerId);

        void ResumeReviewer([NotNull] string reviewerId);

        void MakeReviewerBusy([NotNull] string reviewerId);

        void MakeReviewerAvailable([NotNull] string reviewerId);

        /// <summary>
        ///     Assigns review to a reviewer with the highest debt and updates his stats.
        /// </summary>
        /// <remarks>In case there is more than one reviewer with the highest debt it will assign one of those randomly.</remarks>
        /// <returns>Returns the reviewer that has been assigned for the review.</returns>
        Reviewer AddReviewToHighestDebtor([NotNull] string[] excludedReviewerIds);

        /// <summary>
        ///     Adds one review to given reviewer(s). Ignores their status, so it will assign also to suspended/busy reviewers.
        /// </summary>
        void AddReview([NotNull] string[] reviewerIds);

        /// <summary>
        ///     Removes one review from given reviewers. Ignores their status, so it will un-assign also suspended/busy reviewers.
        /// </summary>
        void RemoveReview([NotNull] string[] reviewerIds);
    }
}