using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using Helpers;

namespace graphconsoleapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = LoadAppSettings();
            if (config == null)
            {
                Console.WriteLine("Invalid appsettings.json file.");
                return;
            }
            var client = GetAuthenticatedGraphClient(config);

            var profileResponse = client.Me.Request().GetAsync().Result;
            Console.WriteLine("Hello " + profileResponse.DisplayName);
            var groupId = "1fe97dca-c561-46a7-b283-9ca4d7699aa7";
            var userOption = ReadUserOption();
            if (userOption == "1")
            {
                // request 1 - all groups
                Console.WriteLine("\n\nREQUEST 1 - ALL GROUPS:");
                var requestAllGroups = client.Groups.Request();
                var resultsAllGroups = requestAllGroups.GetAsync().Result;
                foreach (var group in resultsAllGroups)
                {
                    Console.WriteLine(group.Id + ": " + group.DisplayName + " <" + group.Mail + ">");
                }

                Console.WriteLine("\nGraph Request:");
                Console.WriteLine(requestAllGroups.GetHttpRequestMessage().RequestUri);
                ReadUserOption();
            }
            else if (userOption == "2")
            {
                // request 2 - one group
                Console.WriteLine("\n\nREQUEST 2 - ONE GROUP:");
                var requestGroup = client.Groups[groupId].Request();
                var resultsGroup = requestGroup.GetAsync().Result;
                Console.WriteLine(resultsGroup.Id + ": " + resultsGroup.DisplayName + " <" + resultsGroup.Mail + ">");

                Console.WriteLine("\nGraph Request:");
                Console.WriteLine(requestGroup.GetHttpRequestMessage().RequestUri);
                ReadUserOption();
            }
            else if (userOption == "3")
            {
                // request 3 - group owners
                Console.WriteLine("\n\nREQUEST 3 - GROUP OWNERS:");
                var requestGroupOwners = client.Groups[groupId].Owners.Request();
                var resultsGroupOwners = requestGroupOwners.GetAsync().Result;
                foreach (var owner in resultsGroupOwners)
                {
                    var ownerUser = owner as Microsoft.Graph.User;
                    if (ownerUser != null)
                    {
                        Console.WriteLine(ownerUser.Id + ": " + ownerUser.DisplayName + " <" + ownerUser.Mail + ">");
                    }
                }

                Console.WriteLine("\nGraph Request:");
                Console.WriteLine(requestGroupOwners.GetHttpRequestMessage().RequestUri);
                ReadUserOption();
            }
            else if (userOption == "4")
            {
                // request 4 - group members
                Console.WriteLine("\n\nREQUEST 4 - GROUP MEMBERS:");
                var requestGroupMembers = client.Groups[groupId].Members.Request();
                var resultsGroupMembers = requestGroupMembers.GetAsync().Result;
                foreach (var member in resultsGroupMembers)
                {
                    var memberUser = member as Microsoft.Graph.User;
                    if (memberUser != null)
                    {
                        Console.WriteLine(memberUser.Id + ": " + memberUser.DisplayName + " <" + memberUser.Mail + ">");
                    }
                }

                Console.WriteLine("\nGraph Request:");
                Console.WriteLine(requestGroupMembers.GetHttpRequestMessage().RequestUri);
                ReadUserOption();
            }
            else if (userOption == "5")
            {
                return;
            }
            else
            {
                Console.WriteLine("Invalid option");
            }

        }
        private static string ReadUserOption()
        {
            string option;
            Console.WriteLine("Select Option: \n 1.All groups \n 2.One group \n 3.Group OWners \n 4.Group Members ");
            option = Console.ReadLine();
            return option;

        }
        private static IConfigurationRoot? LoadAppSettings()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                  .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", false, true)
                                  .Build();

                if (string.IsNullOrEmpty(config["applicationId"]) ||
                    string.IsNullOrEmpty(config["tenantId"]))
                {
                    return null;
                }

                return config;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }
        private static IAuthenticationProvider CreateAuthorizationProvider(IConfigurationRoot config)
        {
            var clientId = config["applicationId"];
            var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

            List<string> scopes = new List<string>();
            scopes.Add("https://graph.microsoft.com/.default");

            var cca = PublicClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .WithDefaultRedirectUri()
                                                    .Build();
            return MsalAuthenticationProvider.GetInstance(cca, scopes.ToArray());
        }
        private static GraphServiceClient GetAuthenticatedGraphClient(IConfigurationRoot config)
        {
            var authenticationProvider = CreateAuthorizationProvider(config);
            var graphClient = new GraphServiceClient(authenticationProvider);
            return graphClient;
        }
    }
}