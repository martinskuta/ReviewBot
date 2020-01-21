using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Review.Core.DataModel;
using Review.Core.Services.Exceptions;
using Review.Core.Utility;

namespace Review.Core.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ReviewContext _context;

        public ReviewService() : this(new ReviewContext())
        {
        }

        public ReviewService([NotNull] ReviewContext reviewContext)
        {
            _context = reviewContext ?? throw new ArgumentNullException(nameof(reviewContext));
        }

        public void RegisterReviewer(string reviewerId, string displayName)
        {
            var reviewer = _context.Reviewers.FirstOrDefault(r => r.Id == reviewerId);
            if (reviewer != null) throw new ReviewerAlreadyRegisteredException(reviewerId);

            _context.Reviewers.Add(new Reviewer {Id = reviewerId, Name = displayName});
        }

        public Reviewer GetReviewer(string reviewerId)
        {
            return _context.Reviewers.FirstOrDefault(r => r.Id == reviewerId) ??
                   throw new ReviewerNotRegisteredException(reviewerId);
        }

        public IReadOnlyList<Reviewer> GetAllReviewers()
        {
            return _context.Reviewers.AsReadOnly();
        }

        public void SuspendReviewer(string reviewerId)
        {
            var reviewer = GetReviewer(reviewerId);

            if (reviewer.Status == ReviewerStatus.Suspended) throw new ReviewerAlreadySuspendedException(reviewerId);

            reviewer.Status = ReviewerStatus.Suspended;
        }

        public void MakeReviewerBusy(string reviewerId)
        {
            var reviewer = GetReviewer(reviewerId);

            if (reviewer.Status == ReviewerStatus.Busy) throw new ReviewerAlreadyBusyException(reviewerId);

            if (reviewer.Status == ReviewerStatus.Suspended)
            {
                throw new ReviewerSuspendedCannotBeBusyException(reviewerId);
            }

            reviewer.Status = ReviewerStatus.Busy;
        }

        public void MakeReviewerAvailable(string reviewerId)
        {
            var reviewer = GetReviewer(reviewerId);

            if (reviewer.Status == ReviewerStatus.Available) throw new ReviewerAlreadyAvailableException(reviewerId);

            reviewer.Status = ReviewerStatus.Available;
        }

        public void SetCanApprovePullRequestFlag(string reviewerId, bool canApprovePullRequest)
        {
            var reviewer = GetReviewer(reviewerId);

            reviewer.CanApprovePullRequest = canApprovePullRequest;
        }

        public IReadOnlyList<Reviewer> AddReviewToHighestDebtor(string[] excludedReviewerIds)
        {
            ValidateContextForAssigningReviews();

            var reviewersWithHighestDebt = GetAvailableReviewersWithHighestDebt(excludedReviewerIds);
            if (reviewersWithHighestDebt.IsEmpty()) throw new NoReviewerAvailableException();

            var highestDebtor = reviewersWithHighestDebt.Random();

            //if the randomly chosen reviewer can approve PR we don't need to do anything else
            if (highestDebtor.CanApprovePullRequest)
            {
                AddReview(highestDebtor);
                NormalizeDebt();
                return new[] { highestDebtor };
            }

            //if the randomly chosen one cannot approve PR we have to try to find one additional reviewer that can
            var reviewers = new List<Reviewer>(2) { highestDebtor };

            //if at least one of the reviewers with the highest debt can approve PR, lets add him directly
            if (reviewersWithHighestDebt.Any(r => r.CanApprovePullRequest))
            {
                reviewers.Add(reviewersWithHighestDebt.Where(r => r.CanApprovePullRequest).Random());
            }
            else
            {
                var highestDebtorsThatCanApprove = GetAvailableReviewersWithHighestDebt(excludedReviewerIds, true);
                //We could be strict and fail here, because there is no available reviewer that can approve PR, but for now we let it up to the users of this service
                if (!highestDebtorsThatCanApprove.IsEmpty())
                {
                    reviewers.Add(highestDebtorsThatCanApprove.Random());
                }
            }

            reviewers.ForEach(AddReview);
            NormalizeDebt();
            return reviewers;
        }

        public void AddReview(string[] reviewerIds)
        {
            if (reviewerIds == null) throw new ArgumentNullException(nameof(reviewerIds));
            if (reviewerIds.IsEmpty())
                throw new ArgumentException("At least one reviewer must be provided.", nameof(reviewerIds));

            ValidateContextForAssigningReviews();

            GetReviewers(reviewerIds).ForEach(AddReview);

            NormalizeDebt(reviewerIds);
        }

        public void RemoveReview(string[] reviewerIds)
        {
            if (reviewerIds == null) throw new ArgumentNullException(nameof(reviewerIds));
            if (reviewerIds.IsEmpty())
                throw new ArgumentException("At least one reviewer must be provided.", nameof(reviewerIds));

            ValidateContextForAssigningReviews();

            GetReviewers(reviewerIds).ForEach(RemoveReview);

            NormalizeDebt(reviewerIds);
        }

        private static void AddReview(Reviewer reviewer)
        {
            reviewer.ReviewCount++;
            reviewer.ReviewDebt--;
        }

        private static void RemoveReview(Reviewer reviewer)
        {
            reviewer.ReviewCount--;
            reviewer.ReviewDebt++;
        }

        /// <summary>
        ///     Normalizes the debt for all reviewers, so nobody has negative debt and that the minimum debt is 0, eg if all
        ///     reviewers have debt of 1, it will normalize it to 0.
        /// </summary>
        private void NormalizeDebt(string[] includeEvenIfSuspended = null)
        {
            var minDebt = _context.Reviewers.Select(r => r.ReviewDebt).Min();
            if (minDebt == 0) return;

            var debtToDistribute = minDebt * -1;
            foreach (var reviewer in _context.Reviewers.Where(r =>
                !r.IsSuspended || includeEvenIfSuspended?.Contains(r.Id) == true))
            {
                reviewer.ReviewDebt += debtToDistribute;
            }
        }

        private IList<Reviewer> GetReviewers(string[] reviewerIds)
        {
            return reviewerIds.Distinct().Select(GetReviewer).ToList();
        }

        private IList<Reviewer> GetAvailableReviewersWithHighestDebt(string[] excludedReviewerIds, bool onlyReviewersThatCanApprove = false)
        {
            var highestDebt = 0;
            var highestDebtors = new List<Reviewer>();

            var availableReviewers = onlyReviewersThatCanApprove
                                         ? _context.Reviewers.Where(
                                             reviewer => reviewer.IsAvailable && reviewer.CanApprovePullRequest && excludedReviewerIds?.Contains(reviewer.Id) == false)
                                         : _context.Reviewers.Where(reviewer => reviewer.IsAvailable && excludedReviewerIds?.Contains(reviewer.Id) == false);

            foreach (var reviewer in availableReviewers)
            {
                if (reviewer.ReviewDebt > highestDebt)
                {
                    highestDebt = reviewer.ReviewDebt;
                    highestDebtors.Clear();
                    highestDebtors.Add(reviewer);
                }
                else if (reviewer.ReviewDebt == highestDebt)
                {
                    highestDebtors.Add(reviewer);
                }
            }

            return highestDebtors;
        }

        private void ValidateContextForAssigningReviews()
        {
            if (_context.Reviewers.IsEmpty()) throw new NoReviewerAvailableException();
        }
    }
}