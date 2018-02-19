using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FantomLib
{
    /// <summary>
    /// Call the Android native plugin
    /// http://fantom1x.blog130.fc2.com/blog-entry-273.html
    ///･"fantomPlugin.aar" is required 'Minimum API Level：Android 4.2 (API 17)' or later.
    ///･Rename "AndroidManifest-FullPlugin~.xml" to "AndroidManifest.xml" 
    /// when receive events of hardware volume button or displaying dialog by speech recognizer.
    ///･Recording permission "RECORD_AUDIO" is required to use speech recognizer.
    ///･Text to Speech is required the reading engine and voice data must be installed on the smartphone.
    /// </summary>
#if UNITY_ANDROID
    public static class AndroidPlugin
    {

        //Class full path of plug-in in Java
        public const string ANDROID_PACKAGE = "jp.fantom1x.plugin.android.fantomPlugin";
        public const string ANDROID_SYSTEM = ANDROID_PACKAGE + ".AndroidSystem";



        //==========================================================
        // etc functions

        /// <summary>
        /// Release of cash etc.
        ///(*) It is better to call with "OnDestroy()" for each scene as much as possible.
        /// </summary>
        public static void Release()
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                androidSystem.CallStatic(
                    "release"
                );
            }
            HardKey.ReleaseCache();
        }



        //==========================================================
        // Anrdoid Widget and oher

        /// <summary>
        /// Call Android Toast
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_Toast
        /// (Toast)
        /// https://developer.android.com/reference/android/widget/Toast.html#LENGTH_LONG
        /// </summary>
        /// <param name="message">Message string</param>
        /// <param name="longDuration">Display length : true = Toast.LENGTH_LONG / false = Toast.LENGTH_SHORT</param>
        public static void ShowToast(string message, bool longDuration = false)
        {
            if (string.IsNullOrEmpty(message))
                return;

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showToast",
                    context,
                    message,
                    longDuration ? 1 : 0
                );
            }));
        }



        /// <summary>
        /// Cancel if there is "Android Toast" being displayed. Ignored when not displayed.
        ///･Even when "AndroidPlugin.Release()" is called, it is executed on the native side.
        /// </summary>
        public static void CancelToast()
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                androidSystem.CallStatic(
                    "cancelToast"
                );
            }
        }



        //==========================================================
        // Android Dialogs
        // https://developer.android.com/guide/topics/ui/dialogs.html
        // (AlertDialog)
        // https://developer.android.com/reference/android/app/AlertDialog.html
        //==========================================================


        /// <summary>
        /// Call Android "Yes/No" Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_AlertDialogYN
        ///･"Yes" -> yesValue / "No" -> noValue is returned as the argument of the callback.
        ///･When neither is pressed (clicking outside the dialog -> back to application), 
        /// nothing is returned (not doing anything).
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="yesCaption">String of "Yes" button</param>
        /// <param name="yesValue">Return value when "Yes" button</param>
        /// <param name="noCaption">String of "No" button</param>
        /// <param name="noValue">Return value when "No" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowDialog(string title, string message, string callbackGameObject, string callbackMethod,
            string yesCaption, string yesValue, string noCaption, string noValue, string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showDialog",
                    context,
                    title,
                    message,
                    callbackGameObject,
                    callbackMethod,
                    yesCaption,
                    yesValue,
                    noCaption,
                    noValue,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android "OK" Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_AlertDialogOK
        ///･When pressed the "OK" button or clicked outside the dialog (-> back to application) 
        /// return the same value (resultValue).
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="resultValue">Return value when "OK" button or closed dialog</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowDialog(string title, string message, string callbackGameObject, string callbackMethod,
            string okCaption, string resultValue, string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showDialog",
                    context,
                    title,
                    message,
                    callbackGameObject,
                    callbackMethod,
                    okCaption,
                    resultValue,
                    style
                );
            }));
        }



        /// <summary>
        /// Call Android "Yes/No" Dialog with CheckBox
        /// http://fantom1x.blog130.fc2.com/blog-entry-279.html#fantomPlugin_AlertDialogYNwithCheck
        ///･The check status is returned as a callback argument with ", CHECKED_TRUE" or ", CHECKED_FALSE"
        /// concatenated with the return value (yesValue / noValue).
        ///･When neither is pressed (clicking outside the dialog -> back to application),
        /// nothing is returned (not doing anything).
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="checkBoxText">Text string of check box</param>
        /// <param name="checkBoxTextColor">Text color of check box (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))</param>
        /// <param name="defaultChecked">Initial state of check box (true = On / false = Off)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="yesCaption">String of "Yes" button</param>
        /// <param name="yesValue">Return value when "Yes" button</param>
        /// <param name="noCaption">String of "No" button</param>
        /// <param name="noValue">Return value when "No" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowDialogWithCheckBox(string title, string message, 
            string checkBoxText, int checkBoxTextColor, bool defaultChecked,
            string callbackGameObject, string callbackMethod,
            string yesCaption, string yesValue, string noCaption, string noValue, string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showDialogWithCheckBox",
                    context,
                    title,
                    message,
                    checkBoxText,
                    checkBoxTextColor,
                    defaultChecked,
                    callbackGameObject,
                    callbackMethod,
                    yesCaption,
                    yesValue,
                    noCaption,
                    noValue,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android "Yes/No" Dialog with CheckBox (Unity.Color overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-279.html#fantomPlugin_AlertDialogYNwithCheck
        ///･The check status is returned as a callback argument with ", CHECKED_TRUE" or ", CHECKED_FALSE"
        /// concatenated with the return value (yesValue / noValue).
        ///･When neither is pressed (clicking outside the dialog -> back to application),
        /// nothing is returned (not doing anything).
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="checkBoxText">Text string of check box</param>
        /// <param name="checkBoxTextColor">Text color of check box (Color.clear = not specified: Not clear color)</param>
        /// <param name="defaultChecked">Initial state of check box (true = On / false = Off)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="yesCaption">String of "Yes" button</param>
        /// <param name="yesValue">Return value when "Yes" button</param>
        /// <param name="noCaption">String of "No" button</param>
        /// <param name="noValue">Return value when "No" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowDialogWithCheckBox(string title, string message,
            string checkBoxText, Color checkBoxTextColor, bool defaultChecked,
            string callbackGameObject, string callbackMethod,
            string yesCaption, string yesValue, string noCaption, string noValue, string style = "")
        {
            ShowDialogWithCheckBox(title, message, checkBoxText, checkBoxTextColor.ToIntARGB(), defaultChecked,
                callbackGameObject, callbackMethod, yesCaption, yesValue, noCaption, noValue, style);
        }



        /// <summary>
        /// Call Android "OK" Dialog with CheckBox
        /// http://fantom1x.blog130.fc2.com/blog-entry-279.html#fantomPlugin_AlertDialogOKwithCheck
        ///･The check status is returned as a callback argument with ", CHECKED_TRUE" or ", CHECKED_FALSE"
        /// concatenated with the return value (resultValue).
        ///･When pressed the "OK" button or clicked outside the dialog (-> back to application) 
        /// return the same value (resultValue).
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="checkBoxText">Text string of check box</param>
        /// <param name="checkBoxTextColor">Text color of check box (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))</param>
        /// <param name="defaultChecked">Initial state of check box (true = On / false = Off)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="resultValue">Return value when "OK" button or the dialog is closed</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowDialogWithCheckBox(string title, string message, 
            string checkBoxText, int checkBoxTextColor, bool defaultChecked,
            string callbackGameObject, string callbackMethod,
            string okCaption, string resultValue, string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showDialogWithCheckBox",
                    context,
                    title,
                    message,
                    checkBoxText,
                    checkBoxTextColor,
                    defaultChecked,
                    callbackGameObject,
                    callbackMethod,
                    okCaption,
                    resultValue,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android "OK" Dialog with CheckBox (Unity.Color overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-279.html#fantomPlugin_AlertDialogOKwithCheck
        ///･The check status is returned as a callback argument with ", CHECKED_TRUE" or ", CHECKED_FALSE"
        /// concatenated with the return value (resultValue).
        ///･When pressed the "OK" button or clicked outside the dialog (-> back to application) 
        /// return the same value (resultValue).
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="checkBoxText">Text string of check box</param>
        /// <param name="checkBoxTextColor">Text color of check box (Color.clear = not specified: Not clear color)</param>
        /// <param name="defaultChecked">Initial state of check box (true = On / false = Off)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="resultValue">Return value when "OK" button or closed dialog</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowDialogWithCheckBox(string title, string message,
            string checkBoxText, Color checkBoxTextColor, bool defaultChecked,
            string callbackGameObject, string callbackMethod,
            string okCaption, string resultValue, string style = "")
        {
            ShowDialogWithCheckBox(title, message, checkBoxText, checkBoxTextColor.ToIntARGB(), defaultChecked,
                callbackGameObject, callbackMethod, okCaption, resultValue, style);
        }



        /// <summary>
        /// Call Android Selection Dialog (Return index or items string)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_SelectDialog
        ///･There is no confirmation button.
        ///･When "OK", the string of the choice (items) is returned as it is as the argument of the callback
        /// (or return index ([*]string type) with resultIsIndex=true).
        ///･When clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="resultIsIndex">Flag to set return value as index ([*]string type)</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSelectDialog(string title, string[] items, 
            string callbackGameObject, string callbackMethod, bool resultIsIndex = false, string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showSelectDialog",
                    context,
                    title,
                    items,
                    callbackGameObject,
                    callbackMethod,
                    resultIsIndex,
                    style
                );
            }));
        }



        /// <summary>
        /// Call Android Selection Dialog (Return result value for each item)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_SelectDialog
        ///･There is no confirmation button.
        ///･An element of the result array (resultValues) corresponding to the sequence of choices (items)
        /// is returned as the argument of the callback.
        ///･When clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="resultValues">The element of the selected index (items) becomes the return value</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSelectDialog(string title, string[] items, string callbackGameObject, string callbackMethod, string[] resultValues, string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showSelectDialog",
                    context,
                    title,
                    items,
                    callbackGameObject,
                    callbackMethod,
                    resultValues,
                    style
                );
            }));
        }




        /// <summary>
        /// Call Android Single Choice Dialog (Return index or items string)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_SingleChoiceDialog
        ///･When "OK", the string of the choice (items) is returned as it is as the argument of the callback
        /// (or return index ([*]string type) with resultIsIndex=true).
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="checkedItem">Initial value of index (0~n-1)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="changeCallbackMethod">Method name to callback of value chaged (it is in GameObject)</param>
        /// <param name="resultIsIndex">Flag to set return value as index ([*]string type)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSingleChoiceDialog(string title, string[] items, int checkedItem, 
            string callbackGameObject, string callbackMethod, string changeCallbackMethod, bool resultIsIndex, 
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showSingleChoiceDialog",
                    context,
                    title,
                    items,
                    checkedItem,
                    callbackGameObject,
                    callbackMethod,
                    changeCallbackMethod,
                    resultIsIndex,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }

        //(*) Argument difference overload
        /// <summary>
        /// Call Android Single Choice Dialog (Return index or items string)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_SingleChoiceDialog
        ///･When "OK", the string of the choice (items) is returned as it is as the argument of the callback
        /// (or return index ([*]string type) with resultIsIndex=true).
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="checkedItem">Initial value of index (0~n-1)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="resultIsIndex">Flag to set return value as index ([*]string type)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSingleChoiceDialog(string title, string[] items, int checkedItem, 
            string callbackGameObject, string callbackMethod, bool resultIsIndex = false, 
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowSingleChoiceDialog(title, items, checkedItem, callbackGameObject, callbackMethod, "", resultIsIndex, okCaption, cancelCaption, style);
        }


        /// <summary>
        /// Call Android Single Choice Dialog (Return result value for each item)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_SingleChoiceDialog
        ///･When "OK", the element of the result array (resultValues) corresponding to the sequence of choices (items)
        /// is returned as the argument of the callback.
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="checkedItem">Initial value of index (0~n-1)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="changeCallbackMethod">Method name to callback of value chaged (it is in GameObject)</param>
        /// <param name="resultValues">The element of the selected index (items) becomes the return value</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSingleChoiceDialog(string title, string[] items, int checkedItem,
            string callbackGameObject, string callbackMethod, string changeCallbackMethod, string[] resultValues,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showSingleChoiceDialog",
                    context,
                    title,
                    items,
                    checkedItem,
                    callbackGameObject,
                    callbackMethod,
                    changeCallbackMethod,
                    resultValues,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }

        //(*) Argument difference overload
        /// <summary>
        /// Call Android Single Choice Dialog (Return result value for each item)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_SingleChoiceDialog
        ///･When "OK", the element of the result array (resultValues) corresponding to the sequence of choices (items)
        /// is returned as the argument of the callback.
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="checkedItem">Initial value of index (0~n-1)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="resultValues">The element of the selected index (items) becomes the return value</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSingleChoiceDialog(string title, string[] items, int checkedItem,
            string callbackGameObject, string callbackMethod, string[] resultValues,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowSingleChoiceDialog(title, items, checkedItem, callbackGameObject, callbackMethod, "", resultValues, okCaption, cancelCaption, style);
        }



        /// <summary>
        /// Call Android Multi Choice Dialog (Return index or items string)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_MultiChoiceDialog
        ///･Return only those checked from multiple choices.
        ///･When "OK", the string of the choice is concatenated with a line feed ("\n") and returned as the argument of the callback
        /// (or return index ([*]string type) with resultIsIndex=true).
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="checkedItems">Initial state of checked (Array) (null = nothing)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="changeCallbackMethod">Method name to callback of value chaged (it is in GameObject)</param>
        /// <param name="resultIsIndex">Flag to set return value as index ([*]string type)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowMultiChoiceDialog(string title, string[] items, bool[] checkedItems,
            string callbackGameObject, string callbackMethod, string changeCallbackMethod, bool resultIsIndex,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showMultiChoiceDialog",
                    context,
                    title,
                    items,
                    checkedItems,
                    callbackGameObject,
                    callbackMethod,
                    changeCallbackMethod,
                    resultIsIndex,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }

        //(*) Argument difference overload
        /// <summary>
        /// Call Android Multi Choice Dialog (Return index or items string)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_MultiChoiceDialog
        ///･Return only those checked from multiple choices.
        ///･When "OK", the string of the choice is concatenated with a line feed ("\n") and returned as the argument of the callback
        /// (or return index ([*]string type) with resultIsIndex=true).
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="checkedItems">Initial state of checked (Array) (null = nothing)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="resultIsIndex">Flag to set return value as index ([*]string type)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowMultiChoiceDialog(string title, string[] items, bool[] checkedItems,
            string callbackGameObject, string callbackMethod, bool resultIsIndex = false,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowMultiChoiceDialog(title, items, checkedItems, callbackGameObject, callbackMethod, "", resultIsIndex, okCaption, cancelCaption, style);
        }


        /// <summary>
        /// Call Android Multi Choice Dialog (Return result value for each item)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_MultiChoiceDialog
        ///･Return only those checked from multiple choices.
        ///･When "OK", the elements of the result array (resultValues) corresponding to the sequence of choices (items)
        /// are concatenated with line feed ("\n") and returned as arguments of the callback.
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="checkedItems">Initial state of checked (Array) (null = nothing)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="changeCallbackMethod">Method name to callback of value chaged (it is in GameObject)</param>
        /// <param name="resultValues">The element of the selected index (items) becomes the return value</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowMultiChoiceDialog(string title, string[] items, bool[] checkedItems,
            string callbackGameObject, string callbackMethod, string changeCallbackMethod, string[] resultValues,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showMultiChoiceDialog",
                    context,
                    title,
                    items,
                    checkedItems,
                    callbackGameObject,
                    callbackMethod,
                    changeCallbackMethod,
                    resultValues,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }

        //(*) Argument difference overload
        /// <summary>
        /// Call Android Multi Choice Dialog (Return result value for each item)
        /// http://fantom1x.blog130.fc2.com/blog-entry-274.html#fantomPlugin_MultiChoiceDialog
        ///･Return only those checked from multiple choices.
        ///･When "OK", the elements of the result array (resultValues) corresponding to the sequence of choices (items)
        /// are concatenated with line feed ("\n") and returned as arguments of the callback.
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="items">Choice strings (Array)</param>
        /// <param name="checkedItems">Initial state of checked (Array) (null = nothing)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="resultValues">The element of the selected index (items) becomes the return value</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowMultiChoiceDialog(string title, string[] items, bool[] checkedItems,
            string callbackGameObject, string callbackMethod, string[] resultValues,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowMultiChoiceDialog(title, items, checkedItems, callbackGameObject, callbackMethod, "", resultValues, okCaption, cancelCaption, style);
        }



        /// <summary>
        /// Call Android Switch Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-280.html#fantomPlugin_SwitchDialog
        ///･Depending on the state of the switch, a dialog for acquiring the On/Off state of each item.
        ///･When "OK", the result (true/false) corresponding to the sequence of items is concatenated with a line feed ("\n")
        /// and returned as the argument of the callback.
        ///･When a key is set for each item, the state of the switch (true/false) is returned with '=' like "key=true", "key=false".
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        ///(*) When use the message string, Notice that it will not be displayed if the items overflow the dialog (-> message="").
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string ("" = nothing)</param>
        /// <param name="items">Item strings (Array)</param>
        /// <param name="itemKeys">Item keys (Array) (null = all nothing)</param>
        /// <param name="checkedItems">Initial state of the switches (Array) (null = all off)</param>
        /// <param name="itemsTextColor">Text color of items (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="changeCallbackMethod">Method name to callback of value chaged (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSwitchDialog(string title, string message, 
            string[] items, string[] itemKeys, bool[] checkedItems, int itemsTextColor,
            string callbackGameObject, string callbackMethod, string changeCallbackMethod,
            string okCaption, string cancelCaption, string style)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showSwitchDialog",
                    context,
                    title,
                    message,
                    items,
                    itemKeys,
                    checkedItems,
                    itemsTextColor,
                    callbackGameObject,
                    callbackMethod,
                    changeCallbackMethod,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }

        /// <summary>
        /// Call Android Switch Dialog (Unity.Color overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-280.html#fantomPlugin_SwitchDialog
        ///･Depending on the state of the switch, a dialog for acquiring the On/Off state of each item.
        ///･When "OK", the result (true/false) corresponding to the sequence of items is concatenated with a line feed ("\n")
        /// and returned as the argument of the callback.
        ///･When a key is set for each item, the state of the switch (true/false) is returned with '=' like "key=true", "key=false".
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        ///(*) When use the message string, Notice that it will not be displayed if the items overflow the dialog (-> message="").
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string ("" = nothing)</param>
        /// <param name="items">Item strings (Array)</param>
        /// <param name="itemKeys">Item keys (Array) (null = nothing)</param>
        /// <param name="checkedItems">Initial state of the switches (Array) (null = all off)</param>
        /// <param name="itemsTextColor">Text color of items (Color.clear = not specified: Not clear color)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="changeCallbackMethod">Method name to callback of value chaged (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSwitchDialog(string title, string message, 
            string[] items, string[] itemKeys, bool[] checkedItems, Color itemsTextColor,
            string callbackGameObject, string callbackMethod, string changeCallbackMethod,
            string okCaption, string cancelCaption, string style)
        {
            ShowSwitchDialog(title, message, items, itemKeys, checkedItems, itemsTextColor.ToIntARGB(), 
                callbackGameObject, callbackMethod, changeCallbackMethod, okCaption, cancelCaption, style);
        }

        //(*) Argument difference overload
        /// <summary>
        /// Call Android Switch Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-280.html#fantomPlugin_SwitchDialog
        ///･Depending on the state of the switch, a dialog for acquiring the On/Off state of each item.
        ///･When "OK", the result (true/false) corresponding to the sequence of items is concatenated with a line feed ("\n")
        /// and returned as the argument of the callback.
        ///･When a key is set for each item, the state of the switch (true/false) is returned with '=' like "key=true", "key=false".
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        ///(*) When use the message string, Notice that it will not be displayed if the items overflow the dialog (-> message="").
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string ("" = nothing)</param>
        /// <param name="items">Item strings (Array)</param>
        /// <param name="itemKeys">Item keys (Array) (null = all nothing)</param>
        /// <param name="checkedItems">Initial state of the switches (Array) (null = all off)</param>
        /// <param name="itemsTextColor">Text color of items (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSwitchDialog(string title, string message, 
            string[] items, string[] itemKeys, bool[] checkedItems, int itemsTextColor,
            string callbackGameObject, string callbackMethod, 
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowSwitchDialog(title, message, items, itemKeys, checkedItems, itemsTextColor, 
                callbackGameObject, callbackMethod, "", okCaption, cancelCaption, style);
        }

        //(*) Argument difference overload
        /// <summary>
        /// Call Android Switch Dialog (Unity.Color overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-280.html#fantomPlugin_SwitchDialog
        ///･Depending on the state of the switch, a dialog for acquiring the On/Off state of each item.
        ///･When "OK", the result (true/false) corresponding to the sequence of items is concatenated with a line feed ("\n")
        /// and returned as the argument of the callback.
        ///･When a key is set for each item, the state of the switch (true/false) is returned with '=' like "key=true", "key=false".
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        ///(*) When use the message string, Notice that it will not be displayed if the items overflow the dialog (-> message="").
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string ("" = nothing)</param>
        /// <param name="items">Item strings (Array)</param>
        /// <param name="itemKeys">Item keys (Array) (null = nothing)</param>
        /// <param name="checkedItems">Initial state of the switches (Array) (null = all off)</param>
        /// <param name="itemsTextColor">Text color of items (Color.clear = not specified: Not clear color)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSwitchDialog(string title, string message,
            string[] items, string[] itemKeys, bool[] checkedItems, Color itemsTextColor,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowSwitchDialog(title, message, items, itemKeys, checkedItems, itemsTextColor.ToIntARGB(), 
                callbackGameObject, callbackMethod, "", okCaption, cancelCaption, style);
        }



        /// <summary>
        /// Call Android Slider Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-281.html#fantomPlugin_SliderDialog
        ///･When "OK", the result value corresponding to the sequence of items is concatenated with a line feed ("\n")
        /// and returned as the argument of the callback.
        ///･The result value follows the setting of the number of digits after the decimal point (digits) (it becomes an integer when 0).
        ///･When a key is set for each item, the value is returned with '=' like "key=3", "key=4.5".
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        ///(*) When use the message string, Notice that it will not be displayed if the items overflow the dialog (-> message="").
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string ("" = nothing)</param>
        /// <param name="items">Item strings (Array)</param>
        /// <param name="itemKeys">Item keys (Array) (null = all nothing)</param>
        /// <param name="defValues">Initial values (null = all 0 : 6 digits of integer part + 3 digits after decimal point)</param>
        /// <param name="minValues">Minimum values (null = all 0 : 6 digits of integer part + 3 digits after decimal point)</param>
        /// <param name="maxValues">Maximum values (null = all 100 : 6 digits of integer part + 3 digits after decimal point)</param>
        /// <param name="digits">Number of decimal places (0 = integer, 1~3 = after decimal point)</param>
        /// <param name="itemsTextColor">Text color of items (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="resultCallbackMethod">Method name to result callback when "OK" button pressed (it is in GameObject)</param>
        /// <param name="changeCallbackMethod">Method name to real-time callback when the value of the slider is changed (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSliderDialog(string title, string message, string[] items, string[] itemKeys, 
            float[] defValues, float[] minValues, float[] maxValues, int[] digits, int itemsTextColor,
            string callbackGameObject, string resultCallbackMethod, string changeCallbackMethod = "",
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showSeekBarDialog",
                    context,
                    title,
                    message,
                    items,
                    itemKeys,
                    defValues,
                    minValues,
                    maxValues,
                    digits,
                    itemsTextColor,
                    callbackGameObject,
                    resultCallbackMethod,
                    changeCallbackMethod,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android Slider Dialog (Unity.Color overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-281.html#fantomPlugin_SliderDialog
        ///･When "OK", the result value corresponding to the sequence of items is concatenated with a line feed ("\n")
        /// and returned as the argument of the callback.
        ///･The result value follows the setting of the number of digits after the decimal point (digits) (it becomes an integer when 0).
        ///･When a key is set for each item, the value is returned with '=' like "key=3", "key=4.5".
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        ///(*) When use the message string, Notice that it will not be displayed if the items overflow the dialog (-> message="").
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string ("" = nothing)</param>
        /// <param name="items">Item strings (Array)</param>
        /// <param name="itemKeys">Item keys (Array) (null = all nothing)</param>
        /// <param name="defValues">Initial values (null = all 0 : 6 digits of integer part + 3 digits after decimal point)</param>
        /// <param name="minValues">Minimum values (null = all 0 : 6 digits of integer part + 3 digits after decimal point)</param>
        /// <param name="maxValues">Maximum values (null = all 100 : 6 digits of integer part + 3 digits after decimal point)</param>
        /// <param name="digits">Number of decimal places (0 = integer, 1~3 = after decimal point)</param>
        /// <param name="itemsTextColor">Text color of items (Color.clear = not specified: Not clear color)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="resultCallbackMethod">Method name to result callback when "OK" button pressed (it is in GameObject)</param>
        /// <param name="changeCallbackMethod">Method name to real-time callback when the value of the slider is changed (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSliderDialog(string title, string message, string[] items, string[] itemKeys,
            float[] defValues, float[] minValues, float[] maxValues, int[] digits, Color itemsTextColor, 
            string callbackGameObject, string resultCallbackMethod, string changeCallbackMethod = "",
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowSliderDialog(title, message, items, itemKeys, 
                defValues, minValues, maxValues, digits, itemsTextColor.ToIntARGB(), 
                callbackGameObject, resultCallbackMethod, changeCallbackMethod, 
                okCaption, cancelCaption, style);
        }



        //==========================================================
        //Customizable Dialog

        public const string ANDROID_CUSTOM_DIALOG = ANDROID_PACKAGE + ".AndroidCustomDialog";

        /// <summary>
        /// Call Android Custom Dialog
        ///･Dialog where freely add Text, Switch, Slider, Toggle buttons and Dividing lines.
        ///･The return value is a pair of values ("key=value" + line feed ("\n")) or JSON format ("{"key":"value"}") for a key set for each item (resultIsJson=true:JSON).
        ///･The parameter (dialogItems) of each item (Widgets: DivisorItem, TextItem, SwitchItem, SliderItem, ToggleItem) in one array.
        ///･Generation of each widget is arranged in order from the top. Ignored if there are invalid parameters (or no dialog is generated)
        ///(*) When use the message string, Notice that it will not be displayed if the items overflow the dialog (-> message="").
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string ("" = nothing)</param>
        /// <param name="dialogItems">Parameters of each item (widget) (Array)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to result callback when "OK" button pressed (it is in GameObject)</param>
        /// <param name="resultIsJson">return value in: true=JSON format / false="key=value\n"</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowCustomDialog(string title, string message, DialogItem[] dialogItems,
            string callbackGameObject, string callbackMethod, bool resultIsJson = false,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            if (dialogItems == null || dialogItems.Length == 0)
                return;

            string[] jsons = dialogItems.Select(e => JsonUtility.ToJson(e)).ToArray();
            if (jsons == null || jsons.Length == 0)
                return;

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_CUSTOM_DIALOG);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showCustomDialog",
                    context,
                    title,
                    message,
                    jsons,
                    callbackGameObject,
                    callbackMethod,
                    resultIsJson,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }



        //==========================================================
        // Text Input Dialogs

        /// <summary>
        /// Call Android Single line text input Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-276.html#fantomPlugin_SingleLineTextDialog
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="text">Initial value of text string</param>
        /// <param name="maxLength">Character limit (0 = no limit)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSingleLineTextDialog(string title, string message, string text, int maxLength,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showSingleLineTextDialog",
                    context,
                    title,
                    message,
                    text,
                    maxLength,
                    callbackGameObject,
                    callbackMethod,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android Single line text input Dialog (no message overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-276.html#fantomPlugin_SingleLineTextDialog
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="text">Initial value of text string</param>
        /// <param name="maxLength">Character limit (0 = no limit)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowSingleLineTextDialog(string title, string text, int maxLength,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowSingleLineTextDialog(title, "", text, maxLength, callbackGameObject, callbackMethod, okCaption, cancelCaption, style);
        }



        /// <summary>
        /// Call Android Multi line text input Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-276.html#fantomPlugin_MultiLineTextDialog
        ///･Text entry to include line breaks. The line feed code of the return value is unified to "\n".
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="text">Initial value of text string</param>
        /// <param name="maxLength">Character limit (0 = no limit)</param>
        /// <param name="lines">Number of display lines</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowMultiLineTextDialog(string title, string message, string text, int maxLength, int lines,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showMultiLineTextDialog",
                    context,
                    title,
                    message,
                    text,
                    maxLength,
                    lines,
                    callbackGameObject,
                    callbackMethod,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android Multi line text input Dialog (no message overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-276.html#fantomPlugin_MultiLineTextDialog
        ///･Text entry to include line breaks. The line feed code of the return value is unified to "\n".
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="text">Initial value of text string</param>
        /// <param name="maxLength">Character limit (0 = no limit)</param>
        /// <param name="lines">Number of display lines</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowMultiLineTextDialog(string title, string text, int maxLength, int lines,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowMultiLineTextDialog(title, "", text, maxLength, lines, callbackGameObject, callbackMethod, okCaption, cancelCaption, style);
        }



        /// <summary>
        /// Call Android Numeric text input Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-277.html#fantomPlugin_NumericTextDialog
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="defValue">Initial value</param>
        /// <param name="maxLength">Character limit (0 = no limit) ([*]Including decimal point and sign)</param>
        /// <param name="enableDecimal">true=decimal possible / false=integer only</param>
        /// <param name="enableSign">Possible to input a sign ('-' or '+') at the beginning</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowNumericTextDialog(string title, string message, float defValue, int maxLength, bool enableDecimal, bool enableSign,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showNumericTextDialog",
                    context,
                    title,
                    message,
                    defValue,
                    maxLength,
                    enableDecimal,
                    enableSign,
                    callbackGameObject,
                    callbackMethod,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android Numeric text input Dialog (no message overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-277.html#fantomPlugin_NumericTextDialog
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="defValue">Initial value</param>
        /// <param name="maxLength">Character limit (0 = no limit) ([*]Including decimal point and sign)</param>
        /// <param name="enableDecimal">true=decimal possible / false=integer only</param>
        /// <param name="enableSign">Possible to input a sign ('-' or '+') at the beginning</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowNumericTextDialog(string title, float defValue, int maxLength, bool enableDecimal, bool enableSign,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowNumericTextDialog(title, "", defValue, maxLength, enableDecimal, enableSign, callbackGameObject, callbackMethod, okCaption, cancelCaption, style);
        }



        /// <summary>
        /// Call Android Alpha Numeric text input Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-277.html#fantomPlugin_AlphaNumericTextDialog
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="text">Initial value of text string</param>
        /// <param name="maxLength">Character limit (0 = no limit)。</param>
        /// <param name="addChars">Additional character lists such as symbols ("_-.@": each character, "" = nothing)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowAlphaNumericTextDialog(string title, string message, string text, int maxLength, string addChars,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showAlphaNumericTextDialog",
                    context,
                    title,
                    message,
                    text,
                    maxLength,
                    addChars,
                    callbackGameObject,
                    callbackMethod,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android Alpha Numeric text input Dialog (no message overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-277.html#fantomPlugin_AlphaNumericTextDialog
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="text">Initial value of text string</param>
        /// <param name="maxLength">Character limit (0 = no limit)。</param>
        /// <param name="addChars">Additional character lists such as symbols ("_-.@": each character, "" = nothing)</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowAlphaNumericTextDialog(string title, string text, int maxLength, string addChars,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowAlphaNumericTextDialog(title, "", text, maxLength, addChars, callbackGameObject, callbackMethod, okCaption, cancelCaption, style);
        }



        /// <summary>
        /// Call Android Password text input Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-277.html#fantomPlugin_PasswordTextDialog
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="text">Initial value of text string</param>
        /// <param name="maxLength">Character limit (0 = no limit)。</param>
        /// <param name="numberOnly">true=numeric only</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowPasswordTextDialog(string title, string message, string text, int maxLength, bool numberOnly,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showPasswordTextDialog",
                    context,
                    title,
                    message,
                    text,
                    maxLength,
                    numberOnly,
                    callbackGameObject,
                    callbackMethod,
                    okCaption,
                    cancelCaption,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android Password text input Dialog (no message overload)
        /// http://fantom1x.blog130.fc2.com/blog-entry-277.html#fantomPlugin_PasswordTextDialog
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="text">Initial value of text string</param>
        /// <param name="maxLength">Character limit (0 = no limit)。</param>
        /// <param name="numberOnly">true=numeric only</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="okCaption">String of "OK" button</param>
        /// <param name="cancelCaption">String of "Cancel" button</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowPasswordTextDialog(string title, string text, int maxLength, bool numberOnly,
            string callbackGameObject, string callbackMethod,
            string okCaption = "OK", string cancelCaption = "Cancel", string style = "")
        {
            ShowPasswordTextDialog(title, "", text, maxLength, numberOnly, callbackGameObject, callbackMethod, okCaption, cancelCaption, style);
        }



        //==========================================================
        // Picker Dialogs

        /// <summary>
        /// Call Android DatePicker Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-278.html#fantomPlugin_DataPickerDialog
        /// When pressed the "OK" button, the date is returned as a callback with the specified format (resultDateFormat).
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Date format [Android-Java])
        /// https://developer.android.com/reference/java/text/SimpleDateFormat.html
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="defaultDate">Initial value of date string (like "2017/01/31", "17/1/1")</param>
        /// <param name="resultDateFormat">Return date format (default: "yyyy/MM/dd"->"2017/01/03", ex: "yy-M-d"->"17-1-3" [Android-Java])</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowDatePickerDialog(string defaultDate, string resultDateFormat, 
            string callbackGameObject, string callbackMethod, string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showDatePickerDialog",
                    context,
                    defaultDate,
                    resultDateFormat,
                    callbackGameObject,
                    callbackMethod,
                    style
                );
            }));
        }


        /// <summary>
        /// Call Android TimePicker Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-278.html#fantomPlugin_TimePickerDialog
        /// When pressed the "OK" button, the time is returned as a callback with the specified format (resultTimeFormat).
        ///･When pressed the "Cancel" button or clicked outside the dialog (-> back to application) return nothing.
        /// (Time format [Android-Java])
        /// https://developer.android.com/reference/java/text/SimpleDateFormat.html
        /// (Theme)
        /// https://developer.android.com/reference/android/R.style.html#Theme
        /// </summary>
        /// <param name="defaultTime">Initial value of time string (like "0:00"~"23:59")</param>
        /// <param name="resultTimeFormat">Return time format (default: "HH:mm"->"03:05", ex: "H:mm"->"3:05" [Android-Java])</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
        /// <param name="style">Style applied to dialog (Theme: "android:Theme.DeviceDefault.Dialog.Alert", "android:Theme.DeviceDefault.Light.Dialog.Alert" or etc)</param>
        public static void ShowTimePickerDialog(string defaultTime, string resultTimeFormat,
            string callbackGameObject, string callbackMethod, string style = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showTimePickerDialog",
                    context,
                    defaultTime,
                    resultTimeFormat,
                    callbackGameObject,
                    callbackMethod,
                    style
                );
            }));
        }




        //==========================================================
        // Notification

        /// <summary>
        /// Call Android Notification
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_Notification
        ///(*) Icon in Unity is fixed with resource name "app_icon".
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="iconName">Icon resource name (Unity's default is "app_icon")</param>
        /// <param name="tag">Identification tag (The same tag is overwritten when notified consecutively)</param>
        /// <param name="showTimestamp">Add notification time display</param>
        public static void ShowNotification(string title, string message, string iconName = "app_icon", string tag = "tag", bool showTimestamp = true)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showNotification",
                    context,
                    title,
                    message,
                    iconName,
                    tag,
                    showTimestamp
                );
            }));
        }



        /// <summary>
        /// Call Android Notification (Tap to take action to URI)
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_NotificationToOpenURL
        ///･(Action: Constant Value)
        /// https://developer.android.com/reference/android/content/Intent.html#ACTION_VIEW
        ///(*) Icon in Unity is fixed with resource name "app_icon".
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="iconName">Icon resource name (Unity's default is "app_icon")</param>
        /// <param name="tag">Identification tag (The same tag is overwritten when notified consecutively)</param>
        /// <param name="action">String of Action (ex: "android.intent.action.VIEW")</param>
        /// <param name="uri">URI to action (URL etc.)</param>
        /// <param name="showTimestamp">Add notification time display</param>
        public static void ShowNotificationToActionURI(string title, string message, string iconName = "app_icon", string tag = "tag",
                                                    string action = "android.intent.action.VIEW", string uri = "", bool showTimestamp = true)
        {
            if (string.IsNullOrEmpty(action))
                return;

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showNotificationToActionURI",
                    context,
                    title,
                    message,
                    iconName,
                    tag,
                    action,
                    uri,
                    showTimestamp
                );
            }));
        }


        /// <summary>
        /// Call Android Notification (Tap to take action to open URL)
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_NotificationToOpenURL
        /// </summary>
        /// <param name="title">Title string</param>
        /// <param name="message">Message string</param>
        /// <param name="url">URL to open in browser</param>
        /// <param name="tag">Identification tag (The same tag is overwritten when notified consecutively)</param>
        /// <param name="showTimestamp">Add notification time display</param>
        public static void ShowNotificationToOpenURL(string title, string message, string url, string tag = "tag", bool showTimestamp = true)
        {
            if (string.IsNullOrEmpty(url))
                return;

            ShowNotificationToActionURI(title, message, "app_icon", tag, "android.intent.action.VIEW", url, showTimestamp);
        }




        //==========================================================
        // Start something action

        /// <summary>
        /// Start something Action
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_OpenURL
        ///･(Action: Constant Value)
        /// https://developer.android.com/reference/android/content/Intent.html#ACTION_VIEW
        /// </summary>
        /// <param name="action">String of Action (ex: "android.intent.action.WEB_SEARCH")</param>
        /// <param name="extra">Parameter name to give to the Action (ex: "query")</param>
        /// <param name="query">Value to give to the Action</param>
        public static void StartAction(string action, string extra, string query)
        {
            if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(extra) || string.IsNullOrEmpty(query))
                return;

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "startAction",
                    context,
                    action,
                    extra,
                    query
                );
            }));
        }


        /// <summary>
        /// Start Web Search
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_OpenURL
        ///･The search application differs depending on the setting of the system.
        ///･Same as StartAction("android.intent.action.WEB_SEARCH", "query", "keyword")
        /// </summary>
        /// <param name="query">Search keyword</param>
        public static void StartWebSearch(string query)
        {
            StartAction("android.intent.action.WEB_SEARCH", "query", query);
        }


        /// <summary>
        /// Start Action to URI
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_OpenURL
        ///･(Action: Constant Value)
        /// https://developer.android.com/reference/android/content/Intent.html#ACTION_VIEW
        /// (ex)
        /// StartActionURI("android.intent.action.VIEW", "geo:37.7749,-122.4194?q=restaurants");   //Google Map (Search word: restaurants)
        /// StartActionURI("android.intent.action.VIEW", "google.streetview:cbll=29.9774614,31.1329645&cbp=0,30,0,0,-15");   //Street View
        /// StartActionURI("android.intent.action.SENDTO", "mailto:xxx@example.com");   //Launch mailer
        /// https://developers.google.com/maps/documentation/android-api/intents
        /// </summary>
        /// <param name="action">String of Action (ex: "android.intent.action.VIEW")</param>
        /// <param name="uri">URI to action (URL etc.)</param>
        public static void StartActionURI(string action, string uri)
        {
            if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(uri))
                return;

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "startActionURI",
                    context,
                    action,
                    uri
                );
            }));
        }


        /// <summary>
        /// Open URL (Launch Browser)
        ///･The browser application differs depending on the setting of the system.
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_OpenURL
        ///･Same as StartActionURI("android.intent.action.VIEW", "URL")
        /// </summary>
        /// <param name="url">URL to open in browser</param>
        public static void StartOpenURL(string url)
        {
            StartActionURI("android.intent.action.VIEW", url);
        }





        //==========================================================
        // Speech Recognizer
        //(*) Required "uses-permission android:name="android.permission.RECORD_AUDIO" tag in "AndroidManifest.xml".
        //(*) Rename "AndroidManifest-FullPlugin~.xml" to "AndroidManifest.xml" 
        // http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_SpeechRecognizerDialog
        // http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_SpeechRecognizerListener
        //==========================================================


        /// <summary>
        /// Does the system support speech recognizer?
        /// </summary>
        /// <returns>true = supported</returns>
        public static bool IsSupportedSpeechRecognizer()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
                    {
                        return androidSystem.CallStatic<bool>(
                            "isSupportedSpeechRecognizer",
                            context
                        );
                    }
                }
            }
        }



        /// <summary>
        /// Call Android Speech Recognizer Dialog
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_SpeechRecognizerDialog
        ///･The recognized strings are concatenated with line feed ("\n") and returned as arguments of the callback.
        /// If it fails, it returns an error code string (like "ERROR_").
        ///･(Error code string)
        /// https://developer.android.com/reference/android/speech/SpeechRecognizer.html#ERROR_AUDIO
        ///･Mainly the following error occurs:
        /// ERROR_NO_MATCH : Could not recognize it.
        /// ERROR_SPEECH_TIMEOUT ： Wait for speech time out.
        /// ERROR_RECOGNIZER_BUSY : System is busy (When going on continuously etc.)
        /// ERROR_INSUFFICIENT_PERMISSIONS : There is no permission tag "RECORD_AUDIO" in "AndroidManifest.xml".
        /// ERROR_NETWORK : Could not connect to the network.
        /// </summary>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="resultCallbackMethod">Method name to result callback (it is in GameObject)</param>
        /// <param name="message">Message string</param>
        public static void ShowSpeechRecognizer(string callbackGameObject, string resultCallbackMethod, string message = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "showSpeechRecognizer",
                    context,
                    callbackGameObject,
                    resultCallbackMethod,
                    message
                );
            }));
        }



        /// <summary>
        /// Call Android Speech Recognizer without Dialog (Receive events and results)
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_SpeechRecognizerListener
        ///･The recognized strings are concatenated with line feed ("\n") and returned as arguments of the callback.
        /// If it fails, it returns an error code string (like "ERROR_").
        ///･(Error code string)
        /// https://developer.android.com/reference/android/speech/SpeechRecognizer.html#ERROR_AUDIO
        ///･Mainly the following error occurs:
        /// ERROR_NO_MATCH : Could not recognize it.
        /// ERROR_SPEECH_TIMEOUT ： Wait for speech time out.
        /// ERROR_RECOGNIZER_BUSY : System is busy (When going on continuously etc.)
        /// ERROR_INSUFFICIENT_PERMISSIONS : There is no permission tag "RECORD_AUDIO" in "AndroidManifest.xml".
        /// ERROR_NETWORK : Could not connect to the network.
        /// </summary>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="resultCallbackMethod">Method name to callback when recognition is successful (it is in GameObject)</param>
        /// <param name="errorCallbackMethod">Method name to callback when recognition is failure or error (it is in GameObject)</param>
        /// <param name="readyCallbackMethod">Method name to callback when start waiting for speech recognition (Always "onReadyForSpeech" is returned) (it is in GameObject)</param>
        /// <param name="beginCallbackMethod">Method name to callback when the first voice entered the microphone (Always "onBeginningOfSpeech" is returned) (it is in GameObject)</param>
        public static void StartSpeechRecognizer(string callbackGameObject, string resultCallbackMethod, string errorCallbackMethod,
            string readyCallbackMethod = "", string beginCallbackMethod = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "startSpeechRecognizer",
                    context,
                    callbackGameObject,
                    resultCallbackMethod,
                    errorCallbackMethod,
                    readyCallbackMethod,
                    beginCallbackMethod
                );
            }));
        }



        /// <summary>
        /// Release speech recognizer instance (Also use it to interrupt)
        ///･Even when "AndroidPlugin.Release()" is called, it is executed on the native side.
        /// </summary>
        public static void ReleaseSpeechRecognizer()
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                androidSystem.CallStatic(
                    "releaseSpeechRecognizer"
                );
            }
        }




        //==========================================================
        // Text To Speech
        //(*) Voice data must be installed on the terminal in order to use it.
        // The following are confirmed operation.
        // https://play.google.com/store/apps/details?id=com.google.android.tts
        // https://play.google.com/store/apps/details?id=jp.kddilabs.n2tts
        //==========================================================


        /// <summary>
        /// Call Android Text To Speech
        /// http://fantom1x.blog130.fc2.com/blog-entry-275.html#fantomPlugin_TextToSpeech
        ///･When it is available at the first startup, "SUCCESS_INIT" status is returned to the callback method (statusCallbackMethod).
        ///･(Error code string)
        /// https://developer.android.com/reference/android/speech/tts/TextToSpeech.html#ERROR
        /// Moreover, the following error occurs:
        /// ERROR_LOCALE_NOT_AVAILABLE : There is no voice data corresponding to the system language setting
        /// ERROR_INIT : Initialization failed
        /// ERROR_UTTERANCEPROGRESS_LISTENER_REGISTER : Event listener registration failed
        /// ERROR_UTTERANCEPROGRESS_LISTENER : Some kind of error of event acquisition
        ///･Interrupted event callback (stopCallbackMethod) can only be operated with API 23 (Android 6.0) or higher.
        /// However, it seems that the behavior varies depending on the system, so be careful.
        /// </summary>
        /// <param name="message">Reading text string</param>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="statusCallbackMethod">Method name to callback when returns status (including error)</param>
        /// <param name="startCallbackMethod">Method name to callback when start reading text (Always "onStart" is returned) (it is in GameObject)</param>
        /// <param name="doneCallbackMethod">Method name to callback when finish reading text (Always "onDone" is returned) (it is in GameObject)</param>
        /// <param name="stopCallbackMethod">Method name to callback when interrupted reading text (During playback is "INTERRUPTED". Other than always "onStop" is returned) (it is in GameObject)</param>
        public static void StartTextToSpeech(string message, string callbackGameObject = "", string statusCallbackMethod = "",
                                                string startCallbackMethod = "", string doneCallbackMethod = "", string stopCallbackMethod = "")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM);
            context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                androidSystem.CallStatic(
                    "startTextToSpeech",
                    context,
                    message,
                    callbackGameObject,
                    statusCallbackMethod,
                    startCallbackMethod,
                    doneCallbackMethod,
                    stopCallbackMethod
                );
            }));
        }



        /// <summary>
        /// Initialize Text To Speech (First time only)
        /// http://fantom1x.blog130.fc2.com/blog-entry-275.html#fantomPlugin_TextToSpeech
        ///･When it is available at the first startup, "SUCCESS_INIT" status is returned to the callback method (statusCallbackMethod).
        ///･(Error code string)
        /// https://developer.android.com/reference/android/speech/tts/TextToSpeech.html#ERROR
        /// Moreover, the following error occurs:
        /// ERROR_LOCALE_NOT_AVAILABLE : There is no voice data corresponding to the system language setting
        /// ERROR_INIT : Initialization failed
        /// ERROR_UTTERANCEPROGRESS_LISTENER_REGISTER : Event listener registration failed
        /// ERROR_UTTERANCEPROGRESS_LISTENER : Some kind of error of event acquisition
        /// </summary>
        /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
        /// <param name="statusCallbackMethod">Method name to callback when returns status (including error)</param>
        public static void InitTextToSpeech(string callbackGameObject = "", string statusCallbackMethod = "")
        {
            StartTextToSpeech("", callbackGameObject, statusCallbackMethod);
        }
        [Obsolete("This method name is a typo. Please use 'InitTextToSpeech' instead.")]
        public static void InitSpeechRecognizer(string callbackGameObject = "", string statusCallbackMethod = "")
        {
            StartTextToSpeech("", callbackGameObject, statusCallbackMethod);
        }



        /// <summary>
        /// Interruption of text reading
        /// </summary>
        public static void StopTextToSpeech()
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                androidSystem.CallStatic(
                    "stopTextToSpeech"
                );
            }
        }



        /// <summary>
        /// Get utterance Speed (1.0f: Normal speed)
        /// </summary>
        /// <returns>0.5f~1.0f~2.0f</returns>
        public static float GetTextToSpeechSpeed()
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                return androidSystem.CallStatic<float>(
                    "getTextToSpeechSpeed"
                );
            }
        }


        /// <summary>
        /// Set utterance Speed (1.0f: Normal speed)
        /// </summary>
        /// <param name="newSpeed">0.5f~1.0f~2.0f</param>
        /// <returns>Speed after setting : 0.5f~1.0f~2.0f</returns>
        public static float SetTextToSpeechSpeed(float newSpeed)
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                return androidSystem.CallStatic<float>(
                    "setTextToSpeechSpeed",
                    newSpeed
                );
            }
        }


        /// <summary>
        /// Add utterance Speed (1.0f: Normal speed)
        /// </summary>
        /// <param name="addSpeed">0.5f~1.0f~2.0f</param>
        /// <returns>Speed after addition : 0.5f~1.0f~2.0f</returns>
        public static float AddTextToSpeechSpeed(float addSpeed)
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                return androidSystem.CallStatic<float>(
                    "addTextToSpeechSpeed",
                    addSpeed
                );
            }
        }


        /// <summary>
        /// Reset utterance Speed (-> 1.0f: Normal speed)
        ///･Same as SetTextToSpeechSpeed(1f)
        /// </summary>
        public static float ResetTextToSpeechSpeed()
        {
            return SetTextToSpeechSpeed(1.0f);
        }



        /// <summary>
        /// Get utterance Pitch (1.0f: Normal pitch)
        /// </summary>
        /// <returns>0.5f~1.0f~2.0f</returns>
        public static float GetTextToSpeechPitch()
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                return androidSystem.CallStatic<float>(
                    "getTextToSpeechPitch"
                );
            }
        }


        /// <summary>
        /// Set utterance Pitch (1.0f: Normal pitch)
        /// </summary>
        /// <param name="newPitch">0.5f~1.0f~2.0f</param>
        /// <returns>Pitch after setting : 0.5f~1.0f~2.0f</returns>
        public static float SetTextToSpeechPitch(float newPitch)
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                return androidSystem.CallStatic<float>(
                    "setTextToSpeechPitch",
                    newPitch
                );
            }
        }


        /// <summary>
        /// Add utterance Pitch (1.0f: Normal pitch)
        /// </summary>
        /// <param name="addPitch">Pitch to be added: 0.5f~1.0f~2.0f</param>
        /// <returns>Pitch after addition : 0.5f~1.0f~2.0f</returns>
        public static float AddTextToSpeechPitch(float addPitch)
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                return androidSystem.CallStatic<float>(
                    "addTextToSpeechPitch",
                    addPitch
                );
            }
        }


        /// <summary>
        /// Reset utterance Pitch (-> 1.0f: Normal Pitch)
        ///･Same as SetTextToSpeechPitch(1f)
        /// </summary>
        public static float ResetTextToSpeechPitch()
        {
            return SetTextToSpeechPitch(1.0f);
        }



        /// <summary>
        /// Release Text To Speech instance (Resource release)
        ///･Even when "AndroidPlugin.Release()" is called, it is executed on the native side.
        /// </summary>
        public static void ReleaseTextToSpeech()
        {
            using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
            {
                androidSystem.CallStatic(
                    "releaseTextToSpeech"
                );
            }
        }






        //==========================================================
        // Control of Hardware Volume settings

        /// <summary>
        /// Get Hardware Volume (Media volume only)
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_HardVolumeGetSet
        ///･0~max (max:15? Depends on system?)
        /// </summary>
        /// <returns>Current Hardware Volume</returns>
        public static int GetMediaVolume()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
                    {
                        return androidSystem.CallStatic<int>(
                            "getMediaVolume",
                            context
                        );
                    }
                }
            }
        }



        /// <summary>
        /// Get Maximum Hardware Volume (Media volume only)
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_HardVolumeGetSet
        ///･0~max (max:15? Depends on system?)
        /// </summary>
        /// <returns>Maximum Hardware Volume</returns>
        public static int GetMediaMaxVolume()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
                    {
                        return androidSystem.CallStatic<int>(
                            "getMediaMaxVolume",
                            context
                        );
                    }
                }
            }
        }



        /// <summary>
        /// Set Hardware Volume (Media volume only)
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_HardVolumeGetSet
        ///･0~max (max:15? Depends on system?)
        /// </summary>
        /// <param name="volume">new Hardware Volume</param>
        /// <param name="showUI">true=display system UI</param>
        /// <returns>Hardware Volume after setting</returns>
        public static int SetMediaVolume(int volume, bool showUI)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
                    {
                        return androidSystem.CallStatic<int>(
                            "setMediaVolume",
                            context,
                            volume,
                            showUI
                        );
                    }
                }
            }
        }



        /// <summary>
        /// Add Hardware Volume (Media volume only)
        /// http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_HardVolumeGetSet
        ///･0~max (max:15? Depends on system?)
        /// </summary>
        /// <param name="addVol">Volume to be added</param>
        /// <param name="showUI">true=display system UI</param>
        /// <returns>Hardware Volume after addition</returns>
        public static int AddMediaVolume(int addVol, bool showUI)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaClass androidSystem = new AndroidJavaClass(ANDROID_SYSTEM))
                    {
                        return androidSystem.CallStatic<int>(
                            "addMediaVolume",
                            context,
                            addVol,
                            showUI
                        );
                    }
                }
            }
        }





        //========================================================================
        // Get Hardware button press event 
        //･Rename "AndroidManifest-FullPlugin~.xml" to "AndroidManifest.xml" when receive events of hardware volume button.
        //･Cache objects for Android access to monitor input.
        // Therefore, it is better to register the listener with "OnEnable()" and release it with "OnDisable()".
        // http://fantom1x.blog130.fc2.com/blog-entry-273.html#fantomPlugin_HardVolumeListener
        //========================================================================


        /// <summary>
        /// Get Hardware button press event 
        /// </summary>
        public static class HardKey
        {
            //Class full path of plug-in in Java
            public const string ANDROID_HARDKEY = ANDROID_PACKAGE + ".AndroidHardKey";


            //For Android's AndroidHardKey class acquisition
            private static AndroidJavaClass mAndroidHardKey;    //Cached Object

            private static AndroidJavaClass AndroidHardKey {
                get {
                    if (mAndroidHardKey == null)
                    {
                        mAndroidHardKey = new AndroidJavaClass(ANDROID_HARDKEY);
                    }
                    return mAndroidHardKey;
                }
            }


            /// <summary>
            /// Release cashed object
            ///･Also called "AndroidPlugin.Release()".
            /// </summary>
            public static void ReleaseCache()
            {
                if (mAndroidHardKey != null)
                {
                    mAndroidHardKey.Dispose();
                    mAndroidHardKey = null;
                }
            }


            /// <summary>
            /// Release all listeners
            ///･Also called "AndroidPlugin.Release()".
            /// </summary>
            public static void RemoveAllListeners()
            {
                RemoveKeyVolumeUpListener();
                RemoveKeyVolumeDownListener();
                ReleaseCache();
            }



            /// <summary>
            /// Register the callback (listener) when the hardware volume (media volume only) is increased.
            ///･Only one callback can be registered.
            /// </summary>
            /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
            /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
            /// <param name="resultValue">Return value when volume is raised</param>
            public static void SetKeyVolumeUpListener(string callbackGameObject, string callbackMethod, string resultValue)
            {
                AndroidHardKey.CallStatic(
                    "setKeyVolumeUpListener",
                    callbackGameObject,
                    callbackMethod,
                    resultValue
                );
            }



            /// <summary>
            /// Register the callback (listener) when the hardware volume (media volume only) is decreased.
            ///･Only one callback can be registered.
            /// </summary>
            /// <param name="callbackGameObject">GameObject name in hierarchy to callback</param>
            /// <param name="callbackMethod">Method name to callback (it is in GameObject)</param>
            /// <param name="resultValue">Return value when the volume is lowered</param>
            public static void SetKeyVolumeDownListener(string callbackGameObject, string callbackMethod, string resultValue)
            {
                AndroidHardKey.CallStatic(
                    "setKeyVolumeDownListener",
                    callbackGameObject,
                    callbackMethod,
                    resultValue
                );
            }



            /// <summary>
            /// Release the callback (listener) when the hardware volume (media volume only) is increased.
            ///･Also called "AndroidPlugin.Release()".
            /// </summary>
            public static void RemoveKeyVolumeUpListener()
            {
                AndroidHardKey.CallStatic(
                    "releaseKeyVolumeUpListener"
                );
            }



            /// <summary>
            /// Release the callback (listener) when the hardware volume (media volume only) is decreased.
            ///･Also called "AndroidPlugin.Release()".
            /// </summary>
            public static void RemoveKeyVolumeDownListener()
            {
                AndroidHardKey.CallStatic(
                    "releaseKeyVolumeDownListener"
                );
            }



            /// <summary>
            /// Set whether to control media volume by the smartphone itself by hardware buttons.
            ///･To disable the volume buttons on the smartphone, set it to 'false' if you want to operate only from the Unity side.
            ///･It is possible to receive the event and volume control with "SetMediaVolume()" or "AddMediaVolume()".
            /// </summary>
            /// <param name="enable">true=Volume operation on smartphone / false=Disable volume operation at the smartphone</param>
            public static void SetVolumeOperation(bool enable)
            {
                AndroidHardKey.CallStatic(
                    "setVolumeOperation",
                    enable
                );
            }


        }

    }
#endif



    //==========================================================
    // Item (widget) instance parameters class for Custom Dialog

    //Types of items (widgets)
    [Serializable]
    public enum DialogItemType
    {
        Divisor,
        Text,       //Android-TextView
        Switch,     //Android-Switch
        Slider,     //Android-SeekBar
        Toggle,     //Android-RadioButton
    }

    //Base class for parameters of each item
    [Serializable]
    public abstract class DialogItem
    {
        public string type;         //Types of items (widgets)
        public string key = "";     //Key to be associated with return value

        public DialogItem() { }

        public DialogItem(DialogItemType type, string key = "")
        {
            this.type = type.ToString();
            this.key = key;
        }

        public DialogItem Clone()
        {
            return (DialogItem)MemberwiseClone();
        }
    }

    //Parameters for Dividing Line
    [Serializable]
    public class DivisorItem : DialogItem
    {
        public float lineHeight = 1;    //Line width (unit: dp)
        public int lineColor = 0;       //Line color (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))

        public DivisorItem(float lineHeight, int lineColor = 0)
            : base(DialogItemType.Divisor)
        {
            this.lineHeight = lineHeight;
            this.lineColor = lineColor;
        }

        //Unity.Color overload
        public DivisorItem(float lineHeight, Color lineColor)
            : this(lineHeight, XColor.ToIntARGB(lineColor)) { }

        public new DivisorItem Clone()
        {
            return (DivisorItem)MemberwiseClone();
        }
    }

    //Parameters for Text
    [Serializable]
    public class TextItem : DialogItem
    {
        public string text = "";        //Text string
        public int textColor = 0;       //Text color (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))
        public int backgroundColor = 0; //Background color (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))
        public string align = "";       //Text alignment ("" = not specified, "center", "right" or "left")

        public TextItem(string text, int textColor = 0, int backgroundColor = 0, string align = "")
            : base(DialogItemType.Text)
        {
            this.text = text;
            this.textColor = textColor;
            this.backgroundColor = backgroundColor;
            this.align = align;
        }

        //Unity.Color overload
        public TextItem(string text, Color textColor, Color backgroundColor, string align = "")
            : this(text, XColor.ToIntARGB(textColor), XColor.ToIntARGB(backgroundColor), align) { }

        public TextItem(string text, Color textColor)
            : this(text, XColor.ToIntARGB(textColor), 0, "") { }

        public new TextItem Clone()
        {
            return (TextItem)MemberwiseClone();
        }
    }

    //Parameters for Switch
    [Serializable]
    public class SwitchItem : DialogItem
    {
        public string text = "";        //Text string
        public bool defChecked;         //Initial state of switch (true = On / false = Off)
        public int textColor = 0;       //Text color (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))
        public string changeCallbackMethod = ""; //Method name to real-time callback when the value of the switch is changed (it is in GameObject)

        public SwitchItem(string text, string key, bool defChecked, int textColor = 0, string changeCallbackMethod = "")
            : base(DialogItemType.Switch, key)
        {
            this.text = text;
            this.defChecked = defChecked;
            this.textColor = textColor;
            this.changeCallbackMethod = changeCallbackMethod;
        }

        //Unity.Color overload
        public SwitchItem(string text, string key, bool defChecked, Color textColor, string changeCallbackMethod = "")
            : this(text, key, defChecked, XColor.ToIntARGB(textColor), changeCallbackMethod) { }

        public new SwitchItem Clone()
        {
            return (SwitchItem)MemberwiseClone();
        }
    }

    //Parameters for Slider
    [Serializable]
    public class SliderItem : DialogItem
    {
        public string text = "";        //Text string
        public float value;             //Initial value
        public float min;               //Minimum value
        public float max;               //Maximum value
        public int digit;               //Number of decimal places (0 = integer, 1~3 = after decimal point)
        public int textColor = 0;       //Text color (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))
        public string changeCallbackMethod = ""; //Method name to real-time callback when the value of the slider is changed (it is in GameObject)

        public SliderItem(string text, string key, float value, float min = 0, float max = 100, int digit = 0, int textColor = 0, string changeCallbackMethod = "")
            : base(DialogItemType.Slider, key)
        {
            this.text = text;
            this.value = value;
            this.min = min;
            this.max = max;
            this.digit = digit;
            this.textColor = textColor;
            this.changeCallbackMethod = changeCallbackMethod;
        }

        //Unity.Color overload
        public SliderItem(string text, string key, float value, float min, float max, int digit, Color textColor, string changeCallbackMethod = "")
            : this(text, key, value, min, max, digit, XColor.ToIntARGB(textColor), changeCallbackMethod) { }

        public new SliderItem Clone()
        {
            return (SliderItem)MemberwiseClone();
        }
    }

    //Parameters for Toggle (Group)
    [Serializable]
    public class ToggleItem : DialogItem
    {
        public string[] items;          //Item strings (Array)
        public string[] values;         //Value for each item (Array)
        public string defValue = "";    //Initial value ([*]Prioritize checkedItem)
        public int checkedItem;         //Initial index (nothing defValue or not found defValue)
        public int textColor = 0;       //Text color (0 = not specified: Color format is int32 (AARRGGBB: Android-Java))
        public string changeCallbackMethod = ""; //Method name to real-time callback when the value of the toggle is changed (it is in GameObject)

        //Initial value constructor
        public ToggleItem(string[] items, string key, string[] values, string defValue, int textColor = 0, string changeCallbackMethod = "")
            : base(DialogItemType.Toggle, key)
        {
            this.items = items;
            this.values = values;
            this.defValue = defValue;
            this.textColor = textColor;
            this.changeCallbackMethod = changeCallbackMethod;
        }

        //Initial index constructor
        public ToggleItem(string[] items, string key, string[] values, int checkedItem, int textColor = 0, string changeCallbackMethod = "")
            : base(DialogItemType.Toggle, key)
        {
            this.items = items;
            this.values = values;
            this.checkedItem = checkedItem;
            this.textColor = textColor;
            this.changeCallbackMethod = changeCallbackMethod;
        }

        //Unity.Color overload
        public ToggleItem(string[] items, string key, string[] values, string defValue, Color textColor, string changeCallbackMethod = "")
            : this(items, key, values, defValue, XColor.ToIntARGB(textColor), changeCallbackMethod) { }

        public ToggleItem(string[] items, string key, string[] values, int checkedItem, Color textColor, string changeCallbackMethod = "")
            : this(items, key, values, checkedItem, XColor.ToIntARGB(textColor), changeCallbackMethod) { }

        public new ToggleItem Clone()
        {
            ToggleItem obj = (ToggleItem)MemberwiseClone();
            obj.items = (string[])items.Clone();
            obj.values = (string[])values.Clone();
            return obj;
        }
    }
}

