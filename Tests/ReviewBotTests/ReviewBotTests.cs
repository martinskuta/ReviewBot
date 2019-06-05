#region using

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

#endregion

namespace ReviewBot.Tests
{
    [TestFixture]
    public class ReviewBotTests
    {
        [TestFixture]
        public class RegisterReviewerCommand
        {
            [Test]
            public async Task OnTurnAsync_SenderRegistersOneUserAsReviewer_ExpectUserRegisteredWithCorrectReply()
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(allTimeMessage);

                //Assert
                Assert.That(registerMessage.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is now registered as reviewer."));
                Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n<at>xxx</at> (Available): Reviews: 0, Debt: 0\n\n"));
            }

            [Test]
            public async Task OnTurnAsync_SenderRegistersOneUserTwiceInOneMessage_ExpectUserRegisteredOnlyOnceWithCorrectReply()
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx and @xxx");
                var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(allTimeMessage);

                //Assert
                Assert.That(registerMessage.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is now registered as reviewer."));
                Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n<at>xxx</at> (Available): Reviews: 0, Debt: 0\n\n"));
            }

            [Test]
            public async Task OnTurnAsync_SenderRegistersOneUserTwiceInTwoMessages_ExpectUserRegisteredOnlyOnceWithCorrectReplies()
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                var registerMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                //Act
                await reviewBot.OnTurnAsync(registerMessage1);
                await reviewBot.OnTurnAsync(registerMessage2);
                await reviewBot.OnTurnAsync(allTimeMessage);

                //Assert
                Assert.That(registerMessage1.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is now registered as reviewer."));
                Assert.That(registerMessage2.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is already registered."));
                Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n<at>xxx</at> (Available): Reviews: 0, Debt: 0\n\n"));
            }

            [Test]
            public async Task OnTurnAsync_SenderRegistersThreeUsersAsReviewers_ExpectUsersRegisteredWithCorrectReply()
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx, @yyy and @zzz");
                var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(allTimeMessage);

                //Assert
                Assert.That(registerMessage.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> <at>yyy</at> <at>zzz</at> are now registered as reviewers."));
                Assert.That(
                    allTimeMessage.Responses.Peek().Text,
                    Is.EqualTo(
                        "Ordered by review count:\n\n" +
                        "<at>xxx</at> (Available): Reviews: 0, Debt: 0\n\n" +
                        "<at>yyy</at> (Available): Reviews: 0, Debt: 0\n\n" +
                        "<at>zzz</at> (Available): Reviews: 0, Debt: 0\n\n"));
            }

            [Test]
            public async Task OnTurnAsync_SenderRegistersThreeUsersAsReviewersButTwoWereAlreadyRegistered_ExpectTheTwoUsersNotRegisteredWithCorrectReply()
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx @yyy");
                var registerMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx, @yyy and @zzz");
                var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                //Act
                await reviewBot.OnTurnAsync(registerMessage1);
                await reviewBot.OnTurnAsync(registerMessage2);
                await reviewBot.OnTurnAsync(allTimeMessage);

                //Assert
                Assert.That(registerMessage1.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> <at>yyy</at> are now registered as reviewers."));
                Assert.That(registerMessage2.Responses.Peek().Text, Is.EqualTo("<at>zzz</at> is now registered as reviewer. <at>xxx</at> <at>yyy</at> were already registered."));
                Assert.That(
                    allTimeMessage.Responses.Peek().Text,
                    Is.EqualTo(
                        "Ordered by review count:\n\n" +
                        "<at>xxx</at> (Available): Reviews: 0, Debt: 0\n\n" +
                        "<at>yyy</at> (Available): Reviews: 0, Debt: 0\n\n" +
                        "<at>zzz</at> (Available): Reviews: 0, Debt: 0\n\n"));
            }
        }

        [TestFixture]
        public class FindReviewerCommand
        {
            [TestCase("FEATURE-1234 is ready for @Review")]
            [TestCase("FEATURE-1234 @jira-help is ready for @Review")]
            [TestCase("FEATURE-1234 is ready for @Review @jira-help")]
            public async Task OnTurnAsync_LookingForReviewerOfYourPullRequest_ExpectReviewerWithHighestDebtAssigned(string findReviewerMessageText)
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx, @yyy");
                var addReviewMessage = MSTeamsTurnContext.CreateUserToBotMessage("Add @Review to @xxx");
                var findReviewerMessage = MSTeamsTurnContext.CreateUserToBotMessage(findReviewerMessageText);
                var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(addReviewMessage);
                await reviewBot.OnTurnAsync(findReviewerMessage);
                await reviewBot.OnTurnAsync(allTimeMessage);

                //Assert
                Assert.That(findReviewerMessage.Responses.Peek().Text, Is.EqualTo("<at>Sender</at> assign the review to <at>yyy</at> and don't forget to create pull request!"));
                Assert.That(
                    allTimeMessage.Responses.Peek().Text,
                    Is.EqualTo(
                        "Ordered by review count:\n\n" +
                        "<at>xxx</at> (Available): Reviews: 1, Debt: 0\n\n" +
                        "<at>yyy</at> (Available): Reviews: 1, Debt: 0\n\n"));
            }

            [TestCase("@xxx is looking for @Review of SKYE-1234")]
            [TestCase("@xxx is looking for @Review of SKYE-1234. It is quite small.")]
            [TestCase("@xxx is looking for @Review of SKYE-1234 @jira-help")]
            public async Task OnTurnAsync_FindingReviewForOtherDeveloper_ExpectReviewerWithHighestDebtAssigned(string findReviewerMessageText)
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx, @yyy");
                var findReviewerMessage = MSTeamsTurnContext.CreateUserToBotMessage(findReviewerMessageText);
                var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(findReviewerMessage);
                await reviewBot.OnTurnAsync(allTimeMessage);

                //Assert
                Assert.That(findReviewerMessage.Responses.Peek().Text, Is.EqualTo("<at>Sender</at> assign the review to <at>yyy</at> and don't forget to create pull request!"));
                Assert.That(
                    allTimeMessage.Responses.Peek().Text,
                    Is.EqualTo(
                        "Ordered by review count:\n\n" +
                        "<at>yyy</at> (Available): Reviews: 1, Debt: 0\n\n" +
                        "<at>xxx</at> (Available): Reviews: 0, Debt: 1\n\n"));
            }

            [TestCase("@xxx and @yyy are looking for @Review of SKYE-1234")]
            [TestCase("@xxx and @yyy are looking for @Review of SKYE-1234. It is quite small.")]
            [TestCase("@xxx and @yyy are looking for @Review of SKYE-1234 @jira-help")]
            public async Task OnTurnAsync_FindingReviewForMultipleOtherDevelopers_ExpectReviewerWithHighestDebtAssigned(string findReviewerMessageText)
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx, @yyy and @zzz");
                var findReviewerMessage = MSTeamsTurnContext.CreateUserToBotMessage(findReviewerMessageText);
                var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(findReviewerMessage);
                await reviewBot.OnTurnAsync(allTimeMessage);

                //Assert
                Assert.That(findReviewerMessage.Responses.Peek().Text, Is.EqualTo("<at>Sender</at> assign the review to <at>zzz</at> and don't forget to create pull request!"));
                Assert.That(
                    allTimeMessage.Responses.Peek().Text,
                    Is.EqualTo(
                        "Ordered by review count:\n\n" +
                        "<at>zzz</at> (Available): Reviews: 1, Debt: 0\n\n" +
                        "<at>xxx</at> (Available): Reviews: 0, Debt: 1\n\n" +
                        "<at>yyy</at> (Available): Reviews: 0, Debt: 1\n\n"));
            }

            [TestCase("@xxx @yyy and me are looking for @Review of SKYE-1234")]
            [TestCase("@xxx @yyy and me are looking for @Review of SKYE-1234. It is quite small.")]
            [TestCase("@xxx @yyy and me are looking for @Review of SKYE-1234 @jira-help")]
            public async Task OnTurnAsync_FindingReviewForMultipleOtherDevelopersAndSelf_ExpectReviewerWithHighestDebtAssigned(string findReviewerMessageText)
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx, @yyy and @zzz");
                var registerSelfMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                var findReviewerMessage = MSTeamsTurnContext.CreateUserToBotMessage(findReviewerMessageText);
                var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(registerSelfMessage);
                await reviewBot.OnTurnAsync(findReviewerMessage);
                await reviewBot.OnTurnAsync(allTimeMessage);

                //Assert
                Assert.That(findReviewerMessage.Responses.Peek().Text, Is.EqualTo("<at>Sender</at> assign the review to <at>zzz</at> and don't forget to create pull request!"));
                Assert.That(
                    allTimeMessage.Responses.Peek().Text,
                    Is.EqualTo(
                        "Ordered by review count:\n\n" +
                        "<at>zzz</at> (Available): Reviews: 1, Debt: 0\n\n" +
                        "<at>Sender</at> (Available): Reviews: 0, Debt: 1\n\n" +
                        "<at>xxx</at> (Available): Reviews: 0, Debt: 1\n\n" +
                        "<at>yyy</at> (Available): Reviews: 0, Debt: 1\n\n"));
            }
        }

        [TestFixture]
        public class SuspendCommand
        {
            [TestFixture]
            public class SelfSuspend
            {
                [Test]
                public async Task OnTurnAsync_SelfSuspendingWhenNotRegistered_ExpectNotRegisteredReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(suspendMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but you are not registered as reviewer."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("There are no registered reviewers."));
                }

                [Test]
                public async Task OnTurnAsync_SelfSuspendingWhenRegistered_ExpectSuspendedWithCorrectReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(suspendMessage.Responses.Peek().Text, Is.EqualTo("<at>Sender</at> enjoy your time off! Your review debt won't increase until you are back."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>Sender</at> (Suspended): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_SelfSuspendingWhenAlreadySuspended_ExpectAlreadySuspendedReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var suspendMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var suspendMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage1);
                    await reviewBot.OnTurnAsync(suspendMessage2);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(suspendMessage2.Responses.Peek().Text, Is.EqualTo("Yeah yeah, I know that already."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>Sender</at> (Suspended): Reviews: 0, Debt: 0\n\n"));
                }
            }

            [TestFixture]
            public class SuspendSingleReviewer
            {
                [Test]
                public async Task OnTurnAsync_SuspendingNotRegisteredReviewer_ExpectReviewerNotRegisteredReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(suspendMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but <at>xxx</at> is not registered as reviewer."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("There are no registered reviewers."));
                }

                [Test]
                public async Task OnTurnAsync_SuspendingExistingReviewer_ExpectReviewerSuspendedWithCorrectReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(suspendMessage.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> enjoy your time off! Your review debt won't increase until you are back."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>xxx</at> (Suspended): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_SuspendingAlreadySuspendedReviewer_ExpectReviewerAlreadySuspendedReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var suspendMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var suspendMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage1);
                    await reviewBot.OnTurnAsync(suspendMessage2);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(suspendMessage2.Responses.Peek().Text, Is.EqualTo("Yeah yeah, I know that already."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>xxx</at> (Suspended): Reviews: 0, Debt: 0\n\n"));
                }
            }
        }

        [TestFixture]
        public class MakeAvailableCommand
        {
            [TestFixture]
            public class MakeSelfAvailable
            {
                [Test]
                public async Task OnTurnAsync_MakingSelfAvailableWhenNotRegistered_ExpectNotRegisteredReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var makeAvailableMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am back");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(makeAvailableMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeAvailableMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but you are not registered as reviewer."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("There are no registered reviewers."));
                }

                [Test]
                public async Task OnTurnAsync_MakingSelfAvailableWhenAlreadyAvailable_ExpectYeahIKnowReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var makeAvailableMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am back");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeAvailableMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeAvailableMessage.Responses.Peek().Text, Is.EqualTo("Yeah yeah, I know that already."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>Sender</at> (Available): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_MakingSelfAvailableWhenSuspended_ExpectWelcomeBackToDoingReviewsReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var makeAvailableMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am back");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(makeAvailableMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeAvailableMessage.Responses.Peek().Text, Is.EqualTo("Welcome back <at>Sender</at>! Great to see you doing reviews again."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>Sender</at> (Available): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_MakingSelfAvailableWhenBusy_ExpectWelcomeBackToDoingReviewsReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var makeBusyMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am busy");
                    var makeAvailableMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am back");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeBusyMessage);
                    await reviewBot.OnTurnAsync(makeAvailableMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeAvailableMessage.Responses.Peek().Text, Is.EqualTo("Welcome back <at>Sender</at>! Great to see you doing reviews again."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>Sender</at> (Available): Reviews: 0, Debt: 0\n\n"));
                }
            }

            [TestFixture]
            public class MakeSingleReviewerAvailable
            {
                [Test]
                public async Task OnTurnAsync_ResumingNotRegisteredReviewer_ExpectNotRegisteredReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var makeAvailableMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is back");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(makeAvailableMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeAvailableMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but <at>xxx</at> is not registered as reviewer."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("There are no registered reviewers."));
                }

                [Test]
                public async Task OnTurnAsync_MakingAvailableReviewerThatIsAlreadyAvailable_ExpectYeahIKnowReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var makeAvailableMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is back");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeAvailableMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeAvailableMessage.Responses.Peek().Text, Is.EqualTo("Yeah yeah, I know that already."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>xxx</at> (Available): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_MakingSuspendedReviewerAvailable_ExpectWelcomeBackToDoingReviewsReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var makeAvailableMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is back");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(makeAvailableMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeAvailableMessage.Responses.Peek().Text, Is.EqualTo("Welcome back <at>xxx</at>! Great to see you doing reviews again."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>xxx</at> (Available): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_MakingBusyReviewerAvailable_ExpectWelcomeBackToDoingReviewsReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var makeBusyMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is busy");
                    var makeAvailableMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is back");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeBusyMessage);
                    await reviewBot.OnTurnAsync(makeAvailableMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeAvailableMessage.Responses.Peek().Text, Is.EqualTo("Welcome back <at>xxx</at>! Great to see you doing reviews again."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>xxx</at> (Available): Reviews: 0, Debt: 0\n\n"));
                }
            }
        }

        [TestFixture]
        public class MakeBusyCommand
        {
            [TestFixture]
            public class MakeSelfBusy
            {
                [Test]
                public async Task OnTurnAsync_MakingSelfBusyWhenNotRegistered_ExpectNotRegisteredReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var makeBusyMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am busy");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(makeBusyMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeBusyMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but you are not registered as reviewer."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("There are no registered reviewers."));
                }

                [Test]
                public async Task OnTurnAsync_MakingSelfBusyWhenAvailable_ExpectMadeBusyReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var makeBusyMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am busy");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeBusyMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeBusyMessage.Responses.Peek().Text, Is.EqualTo("Ok <at>Sender</at>. I will not assign you any reviews, so you can get things done."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>Sender</at> (Busy): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_MakingSelfBusyWhenAlreadyBusy_ExpectYouAreAlreadyBusyReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var makeBusyMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am busy");
                    var makeBusyMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am busy");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeBusyMessage1);
                    await reviewBot.OnTurnAsync(makeBusyMessage2);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeBusyMessage2.Responses.Peek().Text, Is.EqualTo("<at>Sender</at>, I know that already. Stop lurking around here and get your things done!"));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>Sender</at> (Busy): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_MakingSelfBusyWhenSuspended_ExpectYouAreSuspendedReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var makeBusyMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var makeBusyMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review I am busy");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeBusyMessage1);
                    await reviewBot.OnTurnAsync(makeBusyMessage2);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(
                        makeBusyMessage2.Responses.Peek().Text,
                        Is.EqualTo("<at>Sender</at>, to my knowledge, you are having time off! Resume yourself first if you are back from your time off."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>Sender</at> (Suspended): Reviews: 0, Debt: 0\n\n"));
                }
            }

            [TestFixture]
            public class MakeSingleReviewerBusy
            {
                [Test]
                public async Task OnTurnAsync_MakingNotRegisteredReviewerBusy_ExpectNotRegisteredReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var makeBusyMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is busy");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(makeBusyMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeBusyMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but <at>xxx</at> is not registered as reviewer."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("There are no registered reviewers."));
                }

                [Test]
                public async Task OnTurnAsync_MakingAvailableReviewerBusy_ExpectReviewerMadeBusyReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var makeBusyMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is busy");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeBusyMessage);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeBusyMessage.Responses.Peek().Text, Is.EqualTo("Ok <at>Sender</at>. I will not assign <at>xxx</at> any reviews."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>xxx</at> (Busy): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_MakingReviewerBusyWhenAlreadyBusy_ExpectReviewerAlreadyBusyReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var makeBusyMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is busy");
                    var makeBusyMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is busy");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeBusyMessage1);
                    await reviewBot.OnTurnAsync(makeBusyMessage2);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(makeBusyMessage2.Responses.Peek().Text, Is.EqualTo("<at>Sender</at>, I know that already. <at>xxx</at> must be really busy!"));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>xxx</at> (Busy): Reviews: 0, Debt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_MakingReviewerBusyWhenSuspended_ExpectReviewerSuspendedReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var makeBusyMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var makeBusyMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review @xxx is busy");
                    var allTimeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review alltime");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(makeBusyMessage1);
                    await reviewBot.OnTurnAsync(makeBusyMessage2);
                    await reviewBot.OnTurnAsync(allTimeMessage);

                    //Assert
                    Assert.That(
                        makeBusyMessage2.Responses.Peek().Text,
                        Is.EqualTo("<at>Sender</at>, to my knowledge, <at>xxx</at> is having time off! So I hope <at>xxx</at> is not busy."));
                    Assert.That(allTimeMessage.Responses.Peek().Text, Is.EqualTo("Ordered by review count:\n\n" + "<at>xxx</at> (Suspended): Reviews: 0, Debt: 0\n\n"));
                }
            }
        }

        [Test]
        public async Task OnTurnAsync_HelpMessageReceived_ExpectHelpReply()
        {
            //Arrange
            var reviewBot = MakeReviewBot();
            var helpMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review help");

            //Act
            await reviewBot.OnTurnAsync(helpMessage);

            //Assert
            Assert.That(
                helpMessage.Responses.Peek().Text,
                Is.EqualTo(
                    "This is what I can do for you:\n\n\n\n" +
                    "Register reviewers: \'@Review register @reviewer1, @reviewer2\' or \'@Review register me\' to register yourself.\n\n" +
                    "Print debt status of active reviewers: @Review status\n\n" +
                    "Print all time stats: @Review alltime\n\n" +
                    "Find reviewer: 'SKYE-1234 is ready for @Review' or '@reviewer is looking for @Review of SKYE-1234' or '@reviewer1, @reviewer2 and me are looking for @Review of SKYE-1234'\n\n" +
                    "Add review: Add @Review to @reviewer1 and @reviewer2\n\n" +
                    "Remove review: Remove @Review from @reviewer1 and @reviewer2\n\n" +
                    "Suspend reviewer: \'@Review suspend @reviewer1\' or \'@Review\' suspend me\' to suspend yourself\n\n" +
                    "Make busy: \'@Review @reviewer is busy\' or \'@Review I am busy\' to make yourself busy\n\n" +
                    "Make available: \'@Review @reviewer1 is back\' or \'@Review I am back\' to make yourself available\n\n" +
                    "Help: @Review help"));
        }

        [Test]
        public async Task OnTurnAsync_UnrecognizedCommandMessageReceived_ExpectHelpReply()
        {
            //Arrange
            var reviewBot = MakeReviewBot();
            var helpMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review nothing");

            //Act
            await reviewBot.OnTurnAsync(helpMessage);

            //Assert
            Assert.That(helpMessage.Responses.Peek().Text, Is.EqualTo("Sorry, I didn't understand your message. Use '@Review help' to see what I understand."));
        }

        private static ReviewBot MakeReviewBot()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(s => s.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

            return new ReviewBot(loggerFactoryMock.Object, new MemoryReviewContextStore());
        }
    }
}