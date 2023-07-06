using System;
using Sirenix.OdinInspector;
//using Unity.Notifications.Android;
using UnityEngine;

public class Android_Notifications : MonoBehaviour
{
    [Serializable]
    private struct NotificationSettings
    {
        public string title;
        public string text;

        [HorizontalGroup("FireTimeGroup", LabelWidth = 50, Width = 30f)]
        public int days;
        [HorizontalGroup("FireTimeGroup", LabelWidth = 50, Width = 30f)]
        public int hours;
        [HorizontalGroup("FireTimeGroup", LabelWidth = 50, Width = 30f)]
        public int minutes;

    }

    [SerializeField] private NotificationSettings[] notifications;

    //private AndroidNotificationChannel _channel;
    private const string mainChannel = "mainChannel";
    
    private void Start()
    {
        SetupNotificationChannel();
        RunNotificationSchedules();
    }

    private void SetupNotificationChannel()
    {
        //_channel = new AndroidNotificationChannel()
        //{
        //    Id = mainChannel,
        //    Name = "Default Channel",
        //    Importance = Importance.Default,
        //    Description = "Generic notifications",
        //};
        
        //AndroidNotificationCenter.RegisterNotificationChannel(_channel);
    }
    
    private void RunNotificationSchedules()
    {
        foreach (var notification in notifications)
        {
            ScheduleNotification(notification);
        }
    }

    private void ScheduleNotification(NotificationSettings notificationSettings)
    {
        var timeToAdd = new TimeSpan(notificationSettings.days, 
            notificationSettings.hours, 
            notificationSettings.minutes, 
            0);
        
        var time = DateTime.Now.Add(timeToAdd);

        //var notification = new AndroidNotification
        //{
        //    Title = notificationSettings.title,
        //    Text = notificationSettings.text,
        //    SmallIcon = "icon_small",
        //    LargeIcon = "icon_large",
        //    FireTime = time
        //};

        //AndroidNotificationCenter.SendNotification(notification, mainChannel);
    }
}
