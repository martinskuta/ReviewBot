using System;
using NUnit.Framework;
using Review.Core.DataModel;
using Review.Core.Services;
using Review.Core.Services.Exceptions;

namespace Review.Core.Tests.Services
{
    [TestFixture]
    public class ReviewServiceTests
    {
        [Test]
        public void AddReview_EmptyReviewerList_ExpectArgumentException()
        {
            //Arrange
            var reviewService = new ReviewService();

            //Act
            //Assert
            Assert.Throws<ArgumentException>(() => reviewService.AddReview(new string[0]));
        }

        [Test]
        public void
            AddReview_FourReviewersTwoAvailableOneBusyAndOneSuspendedAndOneAvailableIsAssignedTwice_ExpectSuspendedNotChangedTheAssignedOneHasReviewCountOfTwoAndZeroDebtAndTheOtherHaveZeroReviewsAndDebtOfTwo()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("assigned-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            reviewService.AddReview(new[] {"assigned-reviewer"});
            reviewService.AddReview(new[] {"assigned-reviewer"});

            //Assert
            var assignedReviewer = reviewService.GetReviewer("assigned-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(2));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(0));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(2));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(2));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void
            AddReview_FourReviewersTwoAvailableOneBusyAndOneSuspendedAndTheBusyOneIsAssignedTwice_ExpectSuspendedNotChangedTheAssignedOneHasReviewCountOfTwoAndZeroDebtAndTheOtherHaveZeroReviewsAndDebtOfTwo()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("available-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            reviewService.AddReview(new[] {"busy-reviewer"});
            reviewService.AddReview(new[] {"busy-reviewer"});

            //Assert
            var assignedReviewer = reviewService.GetReviewer("available-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(2));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(2));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(2));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(0));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void
            AddReview_FourReviewersTwoAvailableOneBusyAndOneSuspendedAndTheSuspendedOneIsAssignedTwice_ExpectSuspendedReviewCountOfTwoAndZeroDebtAndTheOtherHaveZeroReviewsAndDebtOfTwo()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("available-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            reviewService.AddReview(new[] {"suspended-reviewer"});
            reviewService.AddReview(new[] {"suspended-reviewer"});

            //Assert
            var assignedReviewer = reviewService.GetReviewer("available-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(2));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(2));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(2));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(2));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void AddReview_Null_ExpectArgumentNullException()
        {
            //Arrange
            var reviewService = new ReviewService();

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => reviewService.AddReview(null));
        }

        [Test]
        public void
            AddReview_SameReviewerPassedTwice_ExpectUpdatedOnlyOnce()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("assigned-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            reviewService.AddReview(new[] {"assigned-reviewer", "assigned-reviewer"});

            //Assert
            var assignedReviewer = reviewService.GetReviewer("assigned-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(1));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(0));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(1));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(1));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void
            AddReview_ThreeReviewersOneBusyAndAssignmentToAvailableAndBusy_ExpectBothAssignedHaveReviewsOf1AndZeroDebtTheOtherDebtOf1AndZeroReviews()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("reviewer1", "");
            reviewService.RegisterReviewer("reviewer2", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            //Act
            reviewService.AddReview(new[] {"reviewer2", "busy-reviewer"});

            //Assert
            var reviewer1 = reviewService.GetReviewer("reviewer1");
            Assert.That(reviewer1.ReviewCount, Is.EqualTo(0));
            Assert.That(reviewer1.ReviewDebt, Is.EqualTo(1));

            var reviewer2 = reviewService.GetReviewer("reviewer2");
            Assert.That(reviewer2.ReviewCount, Is.EqualTo(1));
            Assert.That(reviewer2.ReviewDebt, Is.EqualTo(0));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(1));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void
            AddReview_TwoReviewersPassedAndOneDoesntExist_ExpectReviewerNotRegisteredExceptionAndStatsNotUpdated()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("assigned-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            //Assert
            Assert.Throws<ReviewerNotRegisteredException>(() =>
                reviewService.AddReview(new[] {"assigned-reviewer", "doesntExist"}));

            var assignedReviewer = reviewService.GetReviewer("assigned-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(0));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(0));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(0));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void AddReviewToHighestDebtor_ContextWithoutReviewers_ExpectNoReviewerAvailableException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            //Act
            //Assert
            Assert.Throws<NoReviewerAvailableException>(() => reviewService.AddReviewToHighestDebtor(new string[0]));
        }

        [Test]
        public void
            AddReviewToHighestDebtor_FourAvailableReviewersAndEveryOneAsksForReview3times_ExpectEveryReviewerHas3ReviewsAndZeroDebt()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            var reviewers = new[] {"reviewer1", "reviewer2", "reviewer3", "reviewer4"};
            foreach (var reviewer in reviewers)
            {
                reviewService.RegisterReviewer(reviewer, "");
            }

            //Act
            for (var i = 0; i < 3; i++)
            {
                foreach (var reviewer in reviewers)
                {
                    reviewService.AddReviewToHighestDebtor(new[] {reviewer});
                }
            }

            //Assert
            var reviewer1 = reviewService.GetReviewer("reviewer1");
            Assert.That(reviewer1.ReviewCount, Is.EqualTo(3));
            Assert.That(reviewer1.ReviewDebt, Is.EqualTo(0));

            var reviewer2 = reviewService.GetReviewer("reviewer2");
            Assert.That(reviewer2.ReviewCount, Is.EqualTo(3));
            Assert.That(reviewer2.ReviewDebt, Is.EqualTo(0));

            var reviewer3 = reviewService.GetReviewer("reviewer3");
            Assert.That(reviewer3.ReviewCount, Is.EqualTo(3));
            Assert.That(reviewer3.ReviewDebt, Is.EqualTo(0));

            var reviewer4 = reviewService.GetReviewer("reviewer4");
            Assert.That(reviewer4.ReviewCount, Is.EqualTo(3));
            Assert.That(reviewer4.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void
            AddReviewToHighestDebtor_FourAvailableReviewersAndOneAsksForReviewSixTimes_ExpectTheOtherThreeToHaveEachTwoReviewsAndTheOneAskingForReviewsToBeInDebtByTwo()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("code-spitting-individual", "");
            reviewService.RegisterReviewer("reviewer1", "");
            reviewService.RegisterReviewer("reviewer2", "");
            reviewService.RegisterReviewer("reviewer3", "");

            //Act
            for (var i = 0; i < 6; i++)
            {
                reviewService.AddReviewToHighestDebtor(new[] {"code-spitting-individual"});
            }

            //Assert
            var onlyCodeWriter = reviewService.GetReviewer("code-spitting-individual");
            Assert.That(onlyCodeWriter.ReviewDebt, Is.EqualTo(2));
            Assert.That(onlyCodeWriter.ReviewCount, Is.EqualTo(0));

            var reviewer1 = reviewService.GetReviewer("reviewer1");
            Assert.That(reviewer1.ReviewDebt, Is.EqualTo(0));
            Assert.That(reviewer1.ReviewCount, Is.EqualTo(2));

            var reviewer2 = reviewService.GetReviewer("reviewer2");
            Assert.That(reviewer2.ReviewDebt, Is.EqualTo(0));
            Assert.That(reviewer2.ReviewCount, Is.EqualTo(2));

            var reviewer3 = reviewService.GetReviewer("reviewer3");
            Assert.That(reviewer3.ReviewDebt, Is.EqualTo(0));
            Assert.That(reviewer3.ReviewCount, Is.EqualTo(2));
        }

        [Test]
        public void
            AddReviewToHighestDebtor_FourReviewersOneAvailableOneBusyOneSuspendedAndOneAvailableButExcluded_ExpectReviewAssignedToAvailableReviewer()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.RegisterReviewer("available-reviewer", "");
            reviewService.RegisterReviewer("excluded-reviewer", "");

            reviewService.MakeReviewerBusy("busy-reviewer");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            var assignedReviewer = reviewService.AddReviewToHighestDebtor(new[] {"excluded-reviewer"});

            //Assert
            Assert.That(assignedReviewer.Id, Is.EqualTo("available-reviewer"));
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(1));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void
            AddReviewToHighestDebtor_FourReviewersTwoAvailableOneBusyAndOneSuspendedAndTwoAvailableAskForReview2Times_ExpectSuspendedWithZeroDebtBusyWithDebtOfTwoAndAvailableOnesWithZeroDebtAndTwoReviews()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("available-reviewer1", "");
            reviewService.RegisterReviewer("available-reviewer2", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");
            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            reviewService.AddReviewToHighestDebtor(new[] {"available-reviewer1"});
            reviewService.AddReviewToHighestDebtor(new[] {"available-reviewer2"});
            reviewService.AddReviewToHighestDebtor(new[] {"available-reviewer1"});
            reviewService.AddReviewToHighestDebtor(new[] {"available-reviewer2"});

            //Assert
            var available1 = reviewService.GetReviewer("available-reviewer1");
            Assert.That(available1.ReviewCount, Is.EqualTo(2));
            Assert.That(available1.ReviewDebt, Is.EqualTo(0));

            var available2 = reviewService.GetReviewer("available-reviewer1");
            Assert.That(available2.ReviewCount, Is.EqualTo(2));
            Assert.That(available2.ReviewDebt, Is.EqualTo(0));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(2));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void AddReviewToHighestDebtor_OneReviewer_ExpectTheOnlyReviewerReturnedAndHisStatsUpdated()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("user-id", "");

            //Act
            var assignedReviewer = reviewService.AddReviewToHighestDebtor(new string[0]);

            //Assert
            Assert.That(assignedReviewer.Id, Is.EqualTo("user-id"));
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(1));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void ctor_ContextIsNull_ExpectArgumentNullException()
        {
            //Arrange
            //Act
            //Assert
            Assert.That(() => new ReviewService(null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetReviewer_NotRegisteredReviewerId_ExpectReviewerNotRegisteredException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            //Act
            //Assert
            Assert.Throws<ReviewerNotRegisteredException>(() => reviewService.GetReviewer("user-id"));
        }

        [Test]
        public void GetReviewer_RegisteredReviewerId_ExpectReviewerReturned()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("user-id", "");

            //Act
            var reviewer = reviewService.GetReviewer("user-id");

            //Assert
            Assert.That(reviewer.Id, Is.EqualTo("user-id"));
        }

        [Test]
        public void MakeReviewerAvailable_ReviewerAvailable_ExpectReviewerAlreadyAvailableException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("user-id", "");

            //Act
            //Assert
            Assert.Throws<ReviewerAlreadyAvailableException>(() => reviewService.MakeReviewerAvailable("user-id"));
        }

        [Test]
        public void MakeReviewerAvailable_ReviewerBusy_ExpectReviewerStatusChangedToAvailable()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("user-id", "");
            reviewService.MakeReviewerBusy("user-id");

            //Act
            reviewService.MakeReviewerAvailable("user-id");

            //Assert
            Assert.That(reviewService.GetReviewer("user-id").Status, Is.EqualTo(ReviewerStatus.Available));
        }

        [Test]
        public void MakeReviewerAvailable_ReviewerDoesntExist_ExpectReviewerNotRegisteredException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            //Act
            //Assert
            Assert.Throws<ReviewerNotRegisteredException>(() => reviewService.MakeReviewerAvailable("user-id"));
        }

        [Test]
        public void MakeReviewerBusy_ReviewerAvailable_ExpectReviewerStatusChangedToBusy()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("user-id", "");

            //Act
            reviewService.MakeReviewerBusy("user-id");

            //Assert
            Assert.That(reviewService.GetReviewer("user-id").Status, Is.EqualTo(ReviewerStatus.Busy));
        }

        [Test]
        public void MakeReviewerBusy_ReviewerBusy_ExpectReviewerAlreadyBusyException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("user-id", "");
            reviewService.MakeReviewerBusy("user-id");

            //Act
            //Assert
            Assert.Throws<ReviewerAlreadyBusyException>(() => reviewService.MakeReviewerBusy("user-id"));
        }

        [Test]
        public void MakeReviewerBusy_ReviewerDoesntExist_ExpectReviewerNotRegisteredException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            //Act
            //Assert
            Assert.Throws<ReviewerNotRegisteredException>(() => reviewService.MakeReviewerBusy("user-id"));
        }

        [Test]
        public void MakeReviewerBusy_ReviewerSuspended_ExpectReviewerSuspendedCannotBeBusyException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("user-id", "");
            reviewService.SuspendReviewer("user-id");

            //Act
            //Assert
            Assert.Throws<ReviewerSuspendedCannotBeBusyException>(() => reviewService.MakeReviewerBusy("user-id"));
        }

        [Test]
        public void RegisterReviewer_ExistingReviewerId_ExpectReviewerAlreadyRegisteredException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("user-id", "");

            //Act
            //Assert
            Assert.Throws<ReviewerAlreadyRegisteredException>(() => reviewService.RegisterReviewer("user-id", ""));
        }

        [Test]
        public void
            RegisterReviewer_NonExistingReviewerId_ExpectNewReviewerAddedWithAvailableStatusAndZeroReviewsAndDebt()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            //Act
            reviewService.RegisterReviewer("user-id", "FirstName LastName");

            //Assert
            Assert.That(reviewContext.Reviewers, Has.Count.EqualTo(1));
            Assert.That(reviewContext.Reviewers[0].Id, Is.EqualTo("user-id"));
            Assert.That(reviewContext.Reviewers[0].Name, Is.EqualTo("FirstName LastName"));
            Assert.That(reviewContext.Reviewers[0].Status, Is.EqualTo(ReviewerStatus.Available));
            Assert.That(reviewContext.Reviewers[0].ReviewCount, Is.EqualTo(0));
            Assert.That(reviewContext.Reviewers[0].ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void RemoveReview_EmptyReviewerList_ExpectArgumentException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("reviewer", "");

            //Act
            //Assert
            Assert.Throws<ArgumentException>(() => reviewService.RemoveReview(new string[0]));
        }

        [Test]
        public void
            RemoveReview_FourReviewersTwoAvailableOneBusyAndOneSuspendedAndOneAvailableHasReviewRemovedTwice_ExpectSuspendedNotChangedTheAssignedOneHasReviewCountOfMinusTwoAndDebtOfTwoAndTheOthersHaveZeroEverything()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("available-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            reviewService.RemoveReview(new[] {"available-reviewer"});
            reviewService.RemoveReview(new[] {"available-reviewer"});

            //Assert
            var assignedReviewer = reviewService.GetReviewer("available-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(-2));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(2));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(0));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(0));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void
            RemoveReview_FourReviewersTwoAvailableOneBusyAndOneSuspendedAndTheBusyOneIsTwoReviewsRemoved_ExpectTheUnassignedOneHasReviewCountOfMinusTwoAndDebtOfTwoAndTheOtherHaveZeroEverything()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("available-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            reviewService.RemoveReview(new[] {"busy-reviewer"});
            reviewService.RemoveReview(new[] {"busy-reviewer"});

            //Assert
            var assignedReviewer = reviewService.GetReviewer("available-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(0));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(0));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(-2));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(2));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void
            RemoveReview_FourReviewersTwoAvailableOneBusyAndOneSuspendedAndTheSuspendedOneHasTwoReviewsRemoved_ExpectSuspendedReviewCountOfMinusTwoAndDebtOfTwoAndTheOthersHaveZeroEverything()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("available-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            reviewService.RemoveReview(new[] {"suspended-reviewer"});
            reviewService.RemoveReview(new[] {"suspended-reviewer"});

            //Assert
            var assignedReviewer = reviewService.GetReviewer("available-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(0));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(0));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(0));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(-2));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(2));
        }

        [Test]
        public void RemoveReview_Null_ExpectArgumentNullException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("reviewer", "");

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => reviewService.RemoveReview(null));
        }

        [Test]
        public void
            RemoveReview_SameReviewerPassedTwice_ExpectUpdatedOnlyOnce()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("assigned-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            reviewService.RemoveReview(new[] {"assigned-reviewer", "assigned-reviewer"});

            //Assert
            var assignedReviewer = reviewService.GetReviewer("assigned-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(-1));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(1));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(0));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(0));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void
            RemoveReview_TwoReviewersPassedAndOneDoesntExist_ExpectReviewerNotRegisteredExceptionAndStatsNotUpdated()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            reviewService.RegisterReviewer("assigned-reviewer", "");
            reviewService.RegisterReviewer("other-available-reviewer", "");

            reviewService.RegisterReviewer("busy-reviewer", "");
            reviewService.MakeReviewerBusy("busy-reviewer");

            reviewService.RegisterReviewer("suspended-reviewer", "");
            reviewService.SuspendReviewer("suspended-reviewer");

            //Act
            //Assert
            Assert.Throws<ReviewerNotRegisteredException>(() =>
                reviewService.RemoveReview(new[] {"assigned-reviewer", "doesntExist"}));

            var assignedReviewer = reviewService.GetReviewer("assigned-reviewer");
            Assert.That(assignedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(assignedReviewer.ReviewDebt, Is.EqualTo(0));

            var availableReviewer = reviewService.GetReviewer("other-available-reviewer");
            Assert.That(availableReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(availableReviewer.ReviewDebt, Is.EqualTo(0));

            var busyReviewer = reviewService.GetReviewer("busy-reviewer");
            Assert.That(busyReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(busyReviewer.ReviewDebt, Is.EqualTo(0));

            var suspendedReviewer = reviewService.GetReviewer("suspended-reviewer");
            Assert.That(suspendedReviewer.ReviewCount, Is.EqualTo(0));
            Assert.That(suspendedReviewer.ReviewDebt, Is.EqualTo(0));
        }

        [Test]
        public void SuspendReviewer_ReviewerAlreadySuspended_ExpectReviewerAlreadySuspendedException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("user-id", "");
            reviewService.SuspendReviewer("user-id");

            //Act
            //Assert
            Assert.Throws<ReviewerAlreadySuspendedException>(() => reviewService.SuspendReviewer("user-id"));
        }

        [Test]
        public void SuspendReviewer_ReviewerAvailable_ExpectReviewersStatusUpdatedToSuspended()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("user-id", "");

            //Act
            reviewService.SuspendReviewer("user-id");

            //Assert
            Assert.That(reviewService.GetReviewer("user-id").Status, Is.EqualTo(ReviewerStatus.Suspended));
        }

        [Test]
        public void SuspendReviewer_ReviewerBusy_ExpectReviewersStatusUpdatedToSuspended()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);
            reviewService.RegisterReviewer("user-id", "");
            reviewService.MakeReviewerBusy("user-id");

            //Act
            reviewService.SuspendReviewer("user-id");

            //Assert
            Assert.That(reviewService.GetReviewer("user-id").Status, Is.EqualTo(ReviewerStatus.Suspended));
        }

        [Test]
        public void SuspendReviewer_ReviewerDoesntExist_ExpectReviewerNotRegisteredException()
        {
            //Arrange
            var reviewContext = new ReviewContext();
            var reviewService = new ReviewService(reviewContext);

            //Act
            //Assert
            Assert.Throws<ReviewerNotRegisteredException>(() => reviewService.SuspendReviewer("user-id"));
        }
    }
}