using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExposureNotification
{
    //A static class of different static variables and methods typically used in the harmony patches
    public static class Helper
    {

        public static LocString DUPE_EXPOSED_TO_GERMS_POPFX = "Exposed to {0}";
        public static LocString DUPE_EXPOSED_TO_GERMS_NOTIFICATION = "Exposed to {0}";
        public static LocString DUPE_EXPOSED_TO_GERMS_TOOLTIP = "The following Duplicants have been exposed to {0}:";
        public static bool MinionsLoaded = false;
        public static bool showLocation = false;


        public static void CreateAndAddNotification(GermExposureMonitor.Instance monitor, string sicknessName)
        {
            string text = string.Format(DUPE_EXPOSED_TO_GERMS_NOTIFICATION, sicknessName);
            Notification.ClickCallback callback = new Notification.ClickCallback(Notification_Callback);
            MinionIdentity minion = monitor.gameObject.GetComponent<MinionIdentity>();
            ShowLocationObject slo = new ShowLocationObject(minion);
            slo.ShowLocation = MinionsLoaded && showLocation;
            Notification notification = new Notification(text, NotificationType.BadMinor, HashedString.Invalid,
                (List<Notification> n, object d) => string.Format(DUPE_EXPOSED_TO_GERMS_TOOLTIP, sicknessName) + n.ReduceMessages(true),
                null, false, 0, callback, slo);
            monitor.gameObject.AddOrGet<Notifier>().Add(notification);
            Action<object> act = null;
            act = x =>
            {
                monitor.gameObject.AddOrGet<Notifier>().Remove(notification);
                monitor.Unsubscribe((int)GameHashes.SleepFinished, act);
                monitor.Unsubscribe((int)GameHashes.DuplicantDied, act);
            };
            monitor.Subscribe((int)GameHashes.SleepFinished, act);
            monitor.Subscribe((int)GameHashes.DuplicantDied, act);
        }


        public static void Notification_Callback(object d)
        {
            ShowLocationObject slo = (ShowLocationObject)d;
            if (slo.ShowLocation)
            {
                CameraController.Instance.CameraGoTo(slo.Pos, 4);
                SelectTool.Instance.Select(slo.Minion.GetComponent<KSelectable>(), true);
            }
            else
            {
                SelectTool.Instance.SelectAndFocus(slo.Minion.transform.GetPosition(), slo.Minion.GetComponent<KSelectable>(), new Vector3(0, 0, 0));
            }
        }
    }
}
