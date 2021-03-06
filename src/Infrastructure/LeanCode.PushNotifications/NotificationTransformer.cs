using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LeanCode.PushNotifications
{
    internal static class NotificationTransformer
    {
        private const string Priority = "high";
        private const int TTL = 28 * 24 * 60 * 60;
        private const string TypeField = "Type";

        public static FCMNotification Convert(
            DeviceType deviceType,
            PushNotification notification,
            PushNotificationsConfiguration? configuration = null)
        {
            return deviceType switch
            {
                DeviceType.Android => ConvertToAndroid(notification),
                DeviceType.iOS => ConvertToiOS(notification),
                DeviceType.Chrome => ConvertToChrome(notification, configuration),

                _ => throw new ArgumentException("Unknown device type.", nameof(deviceType)),
            };
        }

        public static FCMNotification ConvertToAndroid(PushNotification notification)
        {
            return new FCMNotification()
            {
                To = null,
                ContentAvailable = true,
                Priority = Priority,
                TimeToLive = TTL,
                Notification = new FCMNotificationPayload()
                {
                    Title = notification.Title,
                    Body = notification.Content,
                    Sound = "default",
                    Icon = null,
                    Badge = null,
                },
                Data = ConvertData(notification.Data),
            };
        }

        public static FCMNotification ConvertToiOS(PushNotification notification)
        {
            return new FCMNotification()
            {
                To = null,
                ContentAvailable = true,
                Priority = Priority,
                TimeToLive = TTL,
                Notification = new FCMNotificationPayload()
                {
                    Title = notification.Title,
                    Body = notification.Content,
                    Sound = null,
                    Icon = null,
                    Badge = "1",
                },
                Data = ConvertData(notification.Data),
            };
        }

        public static FCMNotification ConvertToChrome(PushNotification notification) =>
            ConvertToChrome(notification, null);

        public static FCMNotification ConvertToChrome(
            PushNotification notification,
            PushNotificationsConfiguration? configuration)
        {
            if (configuration?.UseDataInsteadOfNotification ?? false)
            {
                var data = ConvertData(notification.Data)
                    ?? throw new ArgumentNullException(nameof(notification.Data));

                data.Add("Title", notification.Title);
                data.Add("Content", notification.Content);

                return new FCMNotification()
                {
                    To = null,
                    ContentAvailable = true,
                    Priority = Priority,
                    TimeToLive = TTL,
                    Data = data,
                };
            }

            return new FCMNotification()
            {
                To = null,
                ContentAvailable = true,
                Priority = Priority,
                TimeToLive = TTL,
                Notification = new FCMNotificationPayload()
                {
                    Title = notification.Title,
                    Body = notification.Content,
                    Sound = null,
                    Icon = configuration?.Icon,
                    Badge = null,
                },
                Data = ConvertData(notification.Data),
            };
        }

        [return: NotNullIfNotNull("data")]
        private static Dictionary<string, string?>? ConvertData(object? data)
        {
            if (data is null)
            {
                return null;
            }

            var type = data.GetType();

            var result = new Dictionary<string, string?>()
            {
                [TypeField] = type.Name,
            };

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.Name != TypeField)
                {
                    var value = prop.GetValue(data);

                    if (value != null)
                    {
                        result.Add(prop.Name, value.ToString());
                    }
                }
            }

            return result;
        }
    }
}
