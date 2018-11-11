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
                var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(statusMessage);

                //Assert
                Assert.That(registerMessage.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is now registered as reviewer."));
                Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n<at>xxx</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n"));
            }

            [Test]
            public async Task OnTurnAsync_SenderRegistersOneUserTwiceInOneMessage_ExpectUserRegisteredOnlyOnceWithCorrectReply()
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx and @xxx");
                var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(statusMessage);

                //Assert
                Assert.That(registerMessage.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is now registered as reviewer."));
                Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n<at>xxx</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n"));
            }

            [Test]
            public async Task OnTurnAsync_SenderRegistersOneUserTwiceInTwoMessages_ExpectUserRegisteredOnlyOnceWithCorrectReplies()
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                var registerMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                //Act
                await reviewBot.OnTurnAsync(registerMessage1);
                await reviewBot.OnTurnAsync(registerMessage2);
                await reviewBot.OnTurnAsync(statusMessage);

                //Assert
                Assert.That(registerMessage1.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is now registered as reviewer."));
                Assert.That(registerMessage2.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is already registered."));
                Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n<at>xxx</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n"));
            }

            [Test]
            public async Task OnTurnAsync_SenderRegistersThreeUsersAsReviewers_ExpectUsersRegisteredWithCorrectReply()
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx, @yyy and @zzz");
                var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                //Act
                await reviewBot.OnTurnAsync(registerMessage);
                await reviewBot.OnTurnAsync(statusMessage);

                //Assert
                Assert.That(registerMessage.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> <at>yyy</at> <at>zzz</at> are now registered as reviewers."));
                Assert.That(
                    statusMessage.Responses.Peek().Text,
                    Is.EqualTo(
                        "Ordered by debt:\n\n" +
                        "<at>xxx</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n" +
                        "<at>yyy</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n" +
                        "<at>zzz</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n"));
            }

            [Test]
            public async Task OnTurnAsync_SenderRegistersThreeUsersAsReviewersButTwoWereAlreadyRegistered_ExpectTheTwoUsersNotRegisteredWithCorrectReply()
            {
                //Arrange
                var reviewBot = MakeReviewBot();
                var registerMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx @yyy");
                var registerMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx, @yyy and @zzz");
                var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                //Act
                await reviewBot.OnTurnAsync(registerMessage1);
                await reviewBot.OnTurnAsync(registerMessage2);
                await reviewBot.OnTurnAsync(statusMessage);

                //Assert
                Assert.That(registerMessage1.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> <at>yyy</at> are now registered as reviewers."));
                Assert.That(registerMessage2.Responses.Peek().Text, Is.EqualTo("<at>zzz</at> is now registered as reviewer. <at>xxx</at> <at>yyy</at> were already registered."));
                Assert.That(
                    statusMessage.Responses.Peek().Text,
                    Is.EqualTo(
                        "Ordered by debt:\n\n" +
                        "<at>xxx</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n" +
                        "<at>yyy</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n" +
                        "<at>zzz</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n"));
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
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(suspendMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but you are not registered as reviewer."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("There are no reviewers registered yet."));
                }

                [Test]
                public async Task OnTurnAsync_SelfSuspendingWhenRegistered_ExpectSuspendedWithCorrectReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(suspendMessage.Responses.Peek().Text, Is.EqualTo("<at>Sender</at> enjoy your time off! Your review debt won't increase until you are back."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n" + "<at>Sender</at> (Suspended): ReviewCount: 0, ReviewDebt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_SelfSuspendingWhenAlreadySuspended_ExpectAlreadySuspendedReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var suspendMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var suspendMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage1);
                    await reviewBot.OnTurnAsync(suspendMessage2);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(suspendMessage2.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but you are already suspended."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n" + "<at>Sender</at> (Suspended): ReviewCount: 0, ReviewDebt: 0\n\n"));
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
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(suspendMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but <at>xxx</at> is not registered as reviewer."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("There are no reviewers registered yet."));
                }

                [Test]
                public async Task OnTurnAsync_SuspendingExistingReviewer_ExpectReviewerSuspendedWithCorrectReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(suspendMessage.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> enjoy your time off! Your review debt won't increase until you are back."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n" + "<at>xxx</at> (Suspended): ReviewCount: 0, ReviewDebt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_SuspendingAlreadySuspendedReviewer_ExpectReviewerAlreadySuspendedReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var suspendMessage1 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var suspendMessage2 = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage1);
                    await reviewBot.OnTurnAsync(suspendMessage2);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(suspendMessage2.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but <at>xxx</at> is already suspended."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n" + "<at>xxx</at> (Suspended): ReviewCount: 0, ReviewDebt: 0\n\n"));
                }
            }
        }

        [TestFixture]
        public class ResumeCommand
        {
            [TestFixture]
            public class SelfResume
            {
                [Test]
                public async Task OnTurnAsync_SelfResumingWhenNotRegistered_ExpectNotRegisteredReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var resumeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review resume me");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(resumeMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(resumeMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but you are not registered as reviewer."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("There are no reviewers registered yet."));
                }

                [Test]
                public async Task OnTurnAsync_SelfResumingWhenNotSuspended_ExpectNotSuspendedWithCorrectReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var resumeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review resume me");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(resumeMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(resumeMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but you are not suspended."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n" + "<at>Sender</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_SelfResumingWhenSuspended_ExpectWelcomeBackToDoingReviewsReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register me");
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend me");
                    var resumeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review resume me");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(resumeMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(resumeMessage.Responses.Peek().Text, Is.EqualTo("Welcome back <at>Sender</at>! Great to see you doing reviews again."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n" + "<at>Sender</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n"));
                }
            }

            [TestFixture]
            public class ResumeSingleReviewer
            {
                [Test]
                public async Task OnTurnAsync_ResumingNotRegisteredReviewer_ExpectNotRegisteredReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var resumeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review resume @xxx");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(resumeMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(resumeMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but <at>xxx</at> is not registered as reviewer."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("There are no reviewers registered yet."));
                }

                [Test]
                public async Task OnTurnAsync_ResumingNotSuspendedReviewer_ExpectNotSuspendedWithCorrectReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var resumeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review resume @xxx");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(resumeMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(resumeMessage.Responses.Peek().Text, Is.EqualTo("Sorry <at>Sender</at>, but <at>xxx</at> is not suspended."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n" + "<at>xxx</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n"));
                }

                [Test]
                public async Task OnTurnAsync_ResumingSuspendedReviewer_ExpectWelcomeBackToDoingReviewsReply()
                {
                    //Arrange
                    var reviewBot = MakeReviewBot();
                    var registerMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review register @xxx");
                    var suspendMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review suspend @xxx");
                    var resumeMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review resume @xxx");
                    var statusMessage = MSTeamsTurnContext.CreateUserToBotMessage("@Review status");

                    //Act
                    await reviewBot.OnTurnAsync(registerMessage);
                    await reviewBot.OnTurnAsync(suspendMessage);
                    await reviewBot.OnTurnAsync(resumeMessage);
                    await reviewBot.OnTurnAsync(statusMessage);

                    //Assert
                    Assert.That(resumeMessage.Responses.Peek().Text, Is.EqualTo("Welcome back <at>xxx</at>! Great to see you doing reviews again."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n" + "<at>xxx</at> (Available): ReviewCount: 0, ReviewDebt: 0\n\n"));
                }
            }
        }

        private static ReviewBot MakeReviewBot()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(s => s.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

            return new ReviewBot(loggerFactoryMock.Object, new MemoryReviewContextStore());
        }
    }
}