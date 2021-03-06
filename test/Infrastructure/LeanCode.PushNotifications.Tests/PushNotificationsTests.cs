using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public class PushNotificationsTests
    {
        private readonly IPushNotificationTokenStore<Guid> store;
        private readonly FCMClient client;

        private readonly PushNotifications<Guid> sender;

        public PushNotificationsTests()
        {
            store = Substitute.For<IPushNotificationTokenStore<Guid>>();
            client = Substitute.For<FCMClient>(new HttpClient());

            store.GetForDeviceAsync(Guid.Empty, DeviceType.Android).ReturnsForAnyArgs(Task.FromResult(new List<PushNotificationToken<Guid>>()));
            store.GetAllAsync(Guid.Empty).ReturnsForAnyArgs(Task.FromResult(new List<PushNotificationToken<Guid>>()));

            client.SendAsync(null).ReturnsForAnyArgs(Task.FromResult<FCMResult>(new FCMResult.Success()));

            sender = new PushNotifications<Guid>(null, store, client);
        }

        [Theory]
        [InlineData(DeviceType.Android)]
        [InlineData(DeviceType.iOS)]
        [InlineData(DeviceType.Chrome)]
        public async Task Send_gathers_token_for_device_from_provider(DeviceType deviceType)
        {
            var uid = Guid.NewGuid();

            await sender.SendAsync(uid, deviceType, new PushNotification(string.Empty, string.Empty, null));

            _ = store.Received().GetForDeviceAsync(uid, deviceType);
        }

        [Fact]
        public async Task Send_converts_the_message_assigns_token_and_sends_the_notification_using_FCM()
        {
            const string token = "some token";
            var uid = Guid.NewGuid();

            SetToken(token, uid);

            await sender.SendAsync(uid, DeviceType.Android, new PushNotification(string.Empty, string.Empty, null));

            _ = client.Received(1).SendAsync(Arg.Is<FCMNotification>(n => n.To == token));
        }

        [Fact]
        public async Task Send_sends_the_message_to_all_tokens()
        {
            var uid = Guid.NewGuid();
            var tokens = new List<PushNotificationToken<Guid>>
            {
                new PushNotificationToken<Guid>(Guid.NewGuid(), uid, DeviceType.Android, "a"),
                new PushNotificationToken<Guid>(Guid.NewGuid(), uid, DeviceType.Android, "b"),
            };

            store.GetForDeviceAsync(uid, DeviceType.Android).Returns(tokens);

            await sender.SendAsync(uid, DeviceType.Android, new PushNotification(string.Empty, string.Empty, null));

            foreach (var t in tokens)
            {
                _ = client.Received().SendAsync(Arg.Is<FCMNotification>(n => n.To == t.Token));
            }
        }

        [Fact]
        public async Task Send_converts_the_message_for_Android_correctly()
        {
            var uid = Guid.NewGuid();
            SetToken("token", uid);

            await sender.SendAsync(uid, DeviceType.Android, new PushNotification(string.Empty, string.Empty, null));

            _ = client.Received(1).SendAsync(Arg.Is<FCMNotification>(n => n.Notification.Sound == "default"));
        }

        [Fact]
        public async Task Send_converts_the_message_for_iOS_correctly()
        {
            var uid = Guid.NewGuid();
            SetToken("token", uid, DeviceType.iOS);

            await sender.SendAsync(uid, DeviceType.iOS, new PushNotification(string.Empty, string.Empty, null));

            _ = client.Received(1).SendAsync(Arg.Is<FCMNotification>(n => n.Notification.Badge == "1"));
        }

        [Fact]
        public async Task Send_converts_the_message_for_Chrome_correctly()
        {
            var uid = Guid.NewGuid();
            SetToken("token", uid, DeviceType.Chrome);

            await sender.SendAsync(uid, DeviceType.Chrome, new PushNotification(string.Empty, string.Empty, null));

            _ = client.Received(1).SendAsync(Arg.Is<FCMNotification>(n => n.Notification.Badge == null && n.Notification.Sound == null));
        }

        [Fact]
        public async Task Send_does_nothing_if_token_does_not_exist()
        {
            var uid = Guid.NewGuid();

            await sender.SendAsync(uid, DeviceType.Android, new PushNotification(string.Empty, string.Empty, null));

            _ = client.DidNotReceiveWithAnyArgs().SendAsync(null);
        }

        [Fact]
        public async Task SendToAll_gathers_all_tokens_at_once()
        {
            var uid = Guid.NewGuid();

            await sender.SendToAllAsync(uid, new PushNotification(string.Empty, string.Empty, null));

            _ = store.Received().GetAllAsync(uid);
        }

        [Fact]
        public async Task SendToAll_sends_separate_notifications_for_each_available_device()
        {
            var uid = Guid.NewGuid();

            store.GetAllAsync(uid).Returns(Task.FromResult(new List<PushNotificationToken<Guid>>
            {
                new PushNotificationToken<Guid>(Guid.NewGuid(), uid, DeviceType.Android, "a"),
                new PushNotificationToken<Guid>(Guid.NewGuid(), uid, DeviceType.iOS, "b"),
                new PushNotificationToken<Guid>(Guid.NewGuid(), uid, DeviceType.Chrome, "c"),
            }));

            await sender.SendToAllAsync(uid, new PushNotification(string.Empty, string.Empty, null));

            _ = client.Received(1).SendAsync(Arg.Is<FCMNotification>(n => n.To == "a"));
            _ = client.Received(1).SendAsync(Arg.Is<FCMNotification>(n => n.To == "b"));
            _ = client.Received(1).SendAsync(Arg.Is<FCMNotification>(n => n.To == "c"));
            Assert.Equal(3, client.ReceivedCalls().Count());
        }

        [Fact]
        public async Task SendToAll_does_nothing_if_there_are_no_tokens()
        {
            var uid = Guid.NewGuid();
            await sender.SendToAllAsync(uid, new PushNotification(string.Empty, string.Empty, null));

            _ = client.DidNotReceiveWithAnyArgs().SendAsync(null);
        }

        [Fact]
        public async Task Send_updates_token_if_FCMClient_returns_that_it_has_changed()
        {
            const string newToken = "new-token";
            var uid = Guid.NewGuid();

            var token = SetToken("token", uid);
            client.SendAsync(null).ReturnsForAnyArgs(Task.FromResult<FCMResult>(new FCMResult.TokenUpdated(newToken)));

            await sender.SendAsync(uid, DeviceType.Android, new PushNotification(string.Empty, string.Empty, null));

            _ = store.Received().UpdateTokenAsync(token, newToken);
        }

        [Fact]
        public async Task Send_removes_token_if_FCMClient_returns_that_it_is_invalid()
        {
            var uid = Guid.NewGuid();

            var token = SetToken("token", uid);
            client.SendAsync(null).ReturnsForAnyArgs(Task.FromResult<FCMResult>(new FCMResult.InvalidToken()));

            await sender.SendAsync(uid, DeviceType.Android, new PushNotification(string.Empty, string.Empty, null));

            _ = store.Received().RemoveTokenAsync(token);
        }

        [Fact]
        public Task Send_ignores_HTTP_errors()
        {
            return TestSendResult(new FCMResult.HttpError(HttpStatusCode.BadGateway));
        }

        [Fact]
        public Task Send_ignores_other_error()
        {
            return TestSendResult(new FCMResult.OtherError("other-error"));
        }

        [Fact]
        public Task Send_ignores_successful_result()
        {
            return TestSendResult(new FCMResult.Success());
        }

        private PushNotificationToken<Guid> SetToken(string token, Guid uid, DeviceType deviceType = DeviceType.Android)
        {
            var result = new PushNotificationToken<Guid>(Guid.NewGuid(), uid, deviceType, token);
            store.GetForDeviceAsync(uid, deviceType).Returns(new List<PushNotificationToken<Guid>>
            {
                result,
            });
            return result;
        }

        private async Task TestSendResult(FCMResult result)
        {
            var uid = Guid.NewGuid();

            SetToken("token", uid);
            client.SendAsync(null).ReturnsForAnyArgs(Task.FromResult(result));

            await sender.SendAsync(uid, DeviceType.Android, new PushNotification(string.Empty, string.Empty, null));

            _ = store.DidNotReceiveWithAnyArgs().UpdateTokenAsync(null, null);
            _ = store.DidNotReceiveWithAnyArgs().RemoveTokenAsync(null);
        }
    }
}
