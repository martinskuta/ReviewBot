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

        private static ReviewBot MakeReviewBot()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(s => s.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

            return new ReviewBot(loggerFactoryMock.Object, new MemoryReviewContextStore());
        }
    }
}