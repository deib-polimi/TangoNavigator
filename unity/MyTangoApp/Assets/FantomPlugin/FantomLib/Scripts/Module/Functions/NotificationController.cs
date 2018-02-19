using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FantomLib
{
    /// <summary>
    /// Notification Controller
    /// </summary>
    public class NotificationController : MonoBehaviour
    {
        //Inspector Settings
        public string title = "";                       //When it is empty, the application name is used.
        public string message = "Message";              //Message to be displayed on Notification.

        [Serializable]
        public enum TapAction
        {
            BackToApplication,              //Tap the Notification to return to the application.
            OpenURL,                        //Tap the Notification to open the URL.
        }
        public TapAction tapAction = TapAction.BackToApplication;

        public string url = "";                         //It is opened in the default browser.
        public string iconName = "app_icon";            //Unity defaults to "app_icon" (Unless rewrite it with a 'AndroidManifest.xml' file).
        public string idTag = "tag";                    //Identification tag (When notified consecutively, same tag is displayed by overwriting).
        public bool showTimestamp = true;               //Display notification time.

#region Properties and Local values Section

        //Check empty etc.
        private void CheckForErrors()
        {
            if (tapAction == TapAction.OpenURL && string.IsNullOrEmpty(url))
                Debug.LogWarning("URL is empty.");
        }

#endregion

        // Use this for initialization
        private void Start()
        {
            if (string.IsNullOrEmpty(title))
                title = Application.productName;
#if UNITY_EDITOR
            CheckForErrors();   //Check for fatal errors (Editor only).
#endif
        }

        // Update is called once per frame
        //private void Update()
        //{

        //}

        
        //Show Notification
        public void Show()
        {
#if UNITY_EDITOR
            Debug.Log("NotificationController.Show called");
#elif UNITY_ANDROID
            switch (tapAction)
            {
                case TapAction.BackToApplication:
                    AndroidPlugin.ShowNotification(
                        title, 
                        message,
                        string.IsNullOrEmpty(iconName) ? "app_icon" : iconName,
                        idTag,
                        showTimestamp);
                    break;
                case TapAction.OpenURL:
                    if (!string.IsNullOrEmpty(url))
                        AndroidPlugin.ShowNotificationToActionURI(
                            title,
                            message,
                            string.IsNullOrEmpty(iconName) ? "app_icon" : iconName,
                            idTag,
                            "android.intent.action.VIEW",
                            url,
                            showTimestamp);
                    break;
            }
#endif
        }
    }
}
