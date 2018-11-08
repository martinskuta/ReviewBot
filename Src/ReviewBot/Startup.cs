﻿#region using

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReviewBot.Storage;

#endregion

namespace ReviewBot
{
    /// <summary>
    ///   The Startup class configures services and the request pipeline.
    /// </summary>
    public class Startup
    {
        private readonly bool _isProduction;
        private ILoggerFactory _loggerFactory;

        public Startup(IHostingEnvironment env)
        {
            _isProduction = env.IsProduction();
            var builder = new ConfigurationBuilder()
                          .SetBasePath(env.ContentRootPath)
                          .AddJsonFile("appsettings.json", true, true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                          .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        /// <summary>
        ///   Gets the configuration that represents a set of key/value application configuration properties.
        /// </summary>
        /// <value>
        ///   The <see cref="IConfiguration" /> that represents a set of key/value application configuration properties.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///   This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">
        ///   The <see cref="IServiceCollection" /> specifies the contract for a collection of service
        ///   descriptors.
        /// </param>
        /// <seealso cref="IStatePropertyAccessor{T}" />
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/dependency-injection" />
        /// <seealso
        ///   cref="https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-channels?view=azure-bot-service-4.0" />
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBot<ReviewBot>(
                options =>
                {
                    var secretKey = Configuration.GetSection("botFileSecret")?.Value;
                    var botFilePath = Configuration.GetSection("botFilePath")?.Value;

                    // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
                    var botConfig = BotConfiguration.Load(botFilePath, secretKey);
                    services.AddSingleton(
                        sp =>
                            botConfig ??
                            throw new InvalidOperationException($"The .bot config file could not be loaded. ({botConfig})"));

                    // Retrieve current endpoint.
                    var environment = _isProduction ? "production" : "development";
                    var service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == environment)
                                           .FirstOrDefault();
                    if (!(service is EndpointService endpointService))
                    {
                        throw new InvalidOperationException(
                            $"The .bot file does not contain an endpoint with name '{environment}'.");
                    }

                    options.CredentialProvider =
                        new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                    // Creates a logger for the application to use.
                    ILogger logger = _loggerFactory.CreateLogger<ReviewBot>();

                    // Catches any errors that occur during a conversation turn and logs them.
                    options.OnTurnError = async (context, exception) =>
                    {
                        logger.LogError($"Exception caught : {exception}");
                        await context.SendActivityAsync(
                            $"Sorry, it looks like something went wrong. Error: {exception.Message}");
                    };
                });

            services.AddSingleton<IReviewContextStore, ReviewContextBlobStore>();
            services.AddSingleton<ReviewBot>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
               .UseStaticFiles()
               .UseBotFramework();
        }
    }
}