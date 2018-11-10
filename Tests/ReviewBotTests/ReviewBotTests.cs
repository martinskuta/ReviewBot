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
                    Assert.That(
                        suspendMessage.Responses.Peek().Text,
                        Is.EqualTo("<at>Sender</at>, you are now suspended from reviews. Your review debt won't increase until resumed."));
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
                    Assert.That(suspendMessage2.Responses.Peek().Text, Is.EqualTo("<at>Sender</at>, you are already suspended."));
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
                    Assert.That(suspendMessage.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is now suspended from reviews. xxx's review debt won't increase until resumed."));
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
                    Assert.That(suspendMessage2.Responses.Peek().Text, Is.EqualTo("<at>xxx</at> is already suspended."));
                    Assert.That(statusMessage.Responses.Peek().Text, Is.EqualTo("Ordered by debt:\n\n" + "<at>xxx</at> (Suspended): ReviewCount: 0, ReviewDebt: 0\n\n"));
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