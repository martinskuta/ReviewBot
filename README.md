# Review bot

## What is the bot for?

The main goal is to help developer teams to find reviewer for their code quickly. It also helps to distribute code reviews equally between reviewers and improve sharing of code knowledge.

## What do you mean by 'equally'?

The idea is very simple. The bot is distributing the code reviews in a way that all reviewers do same number of code reviews. In the beginning I wanted to have the bot also measure the 'size'/ complexity of a review, by looking at the changed code and measure the complexity. It turns out that it is not really worth the effort, because it is very hard to objectively define complexity of code review, because the reviews vary. It can be many code changes with automatic refactoring, which are very simple for review, or, on the other side, it can be just one line change that can affect lots of code and requires lots of thinking. So in the end since the size of code reviews is random and if all the reviewers do same number of reviews it will balance the load anyway over time. Some might get unlucky to get two or more big reviews in row, but in a same way they can get lucky and have simple reviews assigned to them, but overtime, since the complexity of reviews is random, my statistics shows that all the reviewers have very similar (almost same) complexity/simple code review ratio (even though the complexity measurment itself is questionable and subjective as I explained).

## How does it help distribute knowledge of code?

Since the reviews are distributed equally between all reviewers, it means, that it doesn't prefer any reviewer that might have prior knowledge of the changed code, so every reviewer will get to know all parts of code, which in the end helps spread the code knowledge and prevents to have one or two developers that know particularly well some parts of code and don't know some other parts at all. Eventhough, if you really want someone specific, you can still assign specific reviewer through the bot.

## Show me some real usage already!

### Review debt

Before that it is still good to know, that the bot works with so called **review debt** instead of real review count, so it is possible that reviewers can join later or leave, without affecting the equal distribution of code reviews between reviewers. You can understand **review debt** as number of reviewes that you owe to a reviewer that does most reviews. If a new reviewer joins the team, he starts with debt of zero. You might get better understanding of review debt by looking at the real usage.

### Supported commands

Please note that @Review is the mention of the bot in a team chat. In any case you can just use '@Review help' to see all available commands.

1. **Registering reviewers**. This allows you to define the reviewers in given context. Context can for example MS Teams channel or Slack channel.
	* Registering yourself: <br> ![reigstering yourself](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Docs/self_registering.png "Self registering")
	* Registering others: <br> ![reigstering others](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Docs/registering_others.png "Registering others")

2. **Assigning reviews**
	* Find reviewer automatically (one with the highest debt): <br> ![assigning review](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Docs/find_reviewer_auto.png "Assigning review automatically")
	* Assign review directly to someone: <br> ![assign review directly](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Docs/add_review.png "Assigning review directly")

3. **Show overall stats**
	* ![showing overall review status](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Docs/review_status.png "Showing overall review status")

5. **Reviewer status commands**
	* **Busy** status. Reviewers can be marked as busy, which means that they have something urgent to do and don't want to have reviews assigned to them by the bot. Their review debt increases during while they are busy. <br> ![making self busy](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Docs/self_busy.png "Marking self busy")
	* **Suspended** status. Reviewers can be suspended from reviews, which means that they are inactive, eg on holidays or not working. During inactive period their debt doesn't increase. <br> ![making self suspended](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Docs/self_suspended.png "Making self suspended")
	* **Available** again. Marking self as available again, after being busy or suspended. <br> ![making self available again](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Docs/self_available.png "Making self available again")

* This is the full list of commands as printed from help command: <br> ![help command](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Docs/help_command.png "Help command")

## Installation

### MS Teams

At the moment the bot is supported only in **MS Teams**. You can either build and deploy the bot yourself or you can start using it for free by using the steps below. Consider the free version as a way to try it, it might not run forever.

1. Download the MS Teams install bundle: [ReviewBotMsTeamsInstallBundle.zip](https://raw.githubusercontent.com/martinskuta/ReviewBot/master/Bundle/ReviewBotMsTeamsInstallBundle.zip "MS Teams installation bundle")
2. Open MS Teams and on the left locate team where you want the bot to be available and click the three dots menu and select **Manage team**
3. On the manage teams page select **Apps** tab.
4. At the bottom you should see a link '**Upload a custom app**'. Click on it and select the bundle you downloaded in first step.
5. Now you can start using the bot by mentioning it in a channel (**@Review help**). Remember that the bot works per channel, so all the review data are stored per channel too, so don't be surprised if it doesn't remember things across channels.

## Techno used

* [.NET Core 2.2](https://github.com/dotnet/core)
* [Bot framework V4](https://dev.botframework.com/)
* [ASP.NET Core 2.2](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-2.1)
* [Azure blob storage](https://azure.microsoft.com/en-us/services/storage/blobs/)
* [Protocol buffers](https://developers.google.com/protocol-buffers/)
* [NUnit testing framework v3](https://nunit.org/)
* [Moq4 mocking framework](https://github.com/Moq/moq4/wiki/Quickstart)

* The bot is hosted in [Azure App Service](https://azure.microsoft.com/en-us/services/app-service/)

## Contact
Feel free to contact me here on github or on [twitter.com/MSkuta](https://twitter.com/MSkuta)
