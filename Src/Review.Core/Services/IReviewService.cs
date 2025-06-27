#region copyright

// Copyright 2007 - 2020 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

#region using

using System.Collections.Generic;
using JetBrains.Annotations;
using Review.Core.DataModel;

#endregion

namespace Review.Core.Services;

public interface IReviewService
{
    void RegisterReviewer([NotNull] string reviewerId, string displayName);

    [NotNull]
    Reviewer GetReviewer([NotNull] string reviewerId);

    IReadOnlyList<Reviewer> GetAllReviewers();

    void UpdateId(string oldId, string newId);

    void SuspendReviewer([NotNull] string reviewerId);

    void MakeReviewerBusy([NotNull] string reviewerId);

    void MakeReviewerAvailable([NotNull] string reviewerId);

    void SetCanApprovePullRequestFlag([NotNull] string reviewerId, bool canApprovePullRequest);

    /// <summary>
    ///   Assigns review to a reviewer with the highest debt and updates his stats. If the highest debtor cannot approve PRs
    ///   then reviewer with highest debt that can approve PRs is returned as well.
    /// </summary>
    /// <remarks>In case there is more than one reviewer with the highest debt it will assign one of those randomly.</remarks>
    /// <returns>Returns the reviewers that has been assigned for the review.</returns>
    IReadOnlyList<Reviewer> AddReviewToHighestDebtor([NotNull] string[] excludedReviewerIds);

    /// <summary>
    ///   Adds one review to given reviewer(s). Ignores their status, so it will assign also to suspended/busy reviewers.
    /// </summary>
    void AddReview([NotNull] string[] reviewerIds);

    /// <summary>
    ///   Removes one review from given reviewers. Ignores their status, so it will un-assign also suspended/busy reviewers.
    /// </summary>
    void RemoveReview([NotNull] string[] reviewerIds);
}