using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.SmsSender.Tests
{
    public class SmsSenderClientTests
    {
        private static readonly SmsApiConfiguration Config = new SmsApiConfiguration
        {
            Login = string.Empty,
            Password = string.Empty,
            From = string.Empty,
            FastMode = false,
            TestMode = false,
        };

        private readonly SmsApiClient client;

        public SmsSenderClientTests()
        {
            client = new SmsApiClient(
                Config,
                new SmsApiHttpClient(
                    new HttpClient
                    {
                        BaseAddress = new Uri(SmsApiClient.ApiBase),
                    }));
        }

        [SuppressMessage("?", "xUnit1004", Justification = "Requires custom data.")]
        [Fact(Skip = "SmsApi credentials required")]
        public async Task Sends_sms_correctly()
        {
            var message = "SmsSender works fine";
            var phoneNumber = string.Empty;
            await client.SendAsync(message, phoneNumber);
        }
    }
}
