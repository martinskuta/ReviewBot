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

        public void ResumeReviewer(string reviewerId)
        {
            var reviewer = GetReviewer(reviewerId);

            if (reviewer.Status != ReviewerStatus.Suspended)
            {
                throw new ReviewerNotSuspendedCannotBeResumedException(reviewerId);
            }

            reviewer.Status = ReviewerStatus.Available;
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

            if (reviewer.Status == ReviewerStatus.Suspended)
            {
                throw new ReviewerSuspendedCannotBeAvailableException(reviewerId);
            }

            reviewer.Status = ReviewerStatus.Available;
        }

        public Reviewer AddReviewToHighestDebtor(string[] excludedReviewerIds)
        {
            ValidateContextForAssigningReviews();

            var reviewersWithHighestDebt = GetAvailableReviewersWithHighestDebt(excludedReviewerIds);
            if (reviewersWithHighestDebt.IsEmpty()) throw new NoReviewerAvailableException();

            var highestDebtor = reviewersWithHighestDebt.Random();

            AddReview(highestDebtor);
            NormalizeDebt();

            return highestDebtor;
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

        private IList<Reviewer> GetAvailableReviewersWithHighestDebt(string[] excludedReviewerIds)
        {
            var highestDebt = 0;
            var highestDebtors = new List<Reviewer>();

            foreach (var reviewer in _context.Reviewers.Where(reviewer =>
                reviewer.IsAvailable && excludedReviewerIds?.Contains(reviewer.Id) == false))
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