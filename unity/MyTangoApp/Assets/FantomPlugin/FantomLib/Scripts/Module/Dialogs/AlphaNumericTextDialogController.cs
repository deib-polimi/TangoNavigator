﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FantomLib
{
    /// <summary>
    /// Alpha Numeric Text Dialog Controller
    ///･Note: Callback from Android to Unity is received under 'GameObject.name'. That is, it is unique within the hierarchy. 
    /// (Theme[Style])
    /// https://developer.android.com/reference/android/R.style.html#Theme
    /// </summary>
    public class AlphaNumericTextDialogController : MonoBehaviour
    {
        //Inspector Settings
        public string title = "Title";                  //Dialog title
        public string message = "Message";              //Dialog message
        [SerializeField] private string text = "Text";  //Default text
        public int maxLength = 0;                       //Limit on number of characters (When it is 0 there is no limit).
        public string addCharacters = "_-";             //List characters such as symbols (Not need a delimiter).

        public string okButton = "OK";                  //Text of 'OK' button.
        public string cancelButton = "Cancel";          //Text of 'Cancel' button.

        public string style = "android:Theme.DeviceDefault.Light.Dialog.Alert"; //Dialog theme

        //Save PlayerPrefs Settings
        public bool saveValue = false;                  //Whether to save the text (Also local value is always overwritten).
        [SerializeField] private string saveKey = "";   //When specifying the PlayerPrefs key for text.

        //Callbacks
        [Serializable] public class ResultHandler : UnityEvent<string> { }  //text string
        public ResultHandler OnResult;

#region PlayerPrefs Section

        //Defalut PlayerPrefs Key (It is used only when saveKey is empty)
        const string TEXT_PREF = "_text";

        //Saved key in PlayerPrefs (Default key is "gameObject.name + '_text'")
        public string SaveKey {
            get { return string.IsNullOrEmpty(saveKey) ? gameObject.name + TEXT_PREF : saveKey; }
        }

        //Load local values manually.
        public void LoadPrefs()
        {
            text = PlayerPrefs.GetString(SaveKey, text);
        }

        //Save local values manually.
        public void SavePrefs()
        {
            PlayerPrefs.SetString(SaveKey, text);
            PlayerPrefs.Save();
        }

        //Delete PlayerPrefs key.
        //Note: Local values are not initialized at this time.
        public void DeletePrefs()
        {
            PlayerPrefs.DeleteKey(SaveKey);
        }

        //Returns true if the saved value exists.
        public bool HasPrefs {
            get { return PlayerPrefs.HasKey(SaveKey); }
        }

#endregion

#region Properties and Local values Section

        //The currently (default) text.
        //･If saveValue is true, it will be automatically saved.
        public string CurrentValue {
            get { return text; }
            set {
                text = string.IsNullOrEmpty(value) ? "" : value;
                if (saveValue)
                    SavePrefs();
            }
        }


        //The value for reset.
        private string initValue;

        //Store the values of the inspector.
        private void StoreInitValue()
        {
            initValue = text;
        }

        //Restore the value of inspector and delete PlayerPrefs key.
        public void ResetValue()
        {
            text = initValue;
            DeletePrefs();
        }

#endregion

        // Use this for initialization
        private void Awake()
        {
            StoreInitValue();

            if (saveValue)
                LoadPrefs();
        }

        private void Start()
        {

        }

        // Update is called once per frame
        //private void Update()
        //{

        //}


        //Show dialog
        public void Show()
        {
#if UNITY_EDITOR
            Debug.Log("AlphaNumericTextDialogController.Show called");
#elif UNITY_ANDROID
            AndroidPlugin.ShowAlphaNumericTextDialog(
                title,
                message,
                text,
                maxLength,
                addCharacters,
                gameObject.name, "ReceiveResult",
                okButton, cancelButton,
                style);
#endif
        }

        //Set text dynamically and show dialog (current text will be overwritten)
        //Note: When changed dynamically, it is inconsistent with the value saved in Playerprefs (better to use saveValue is false).
        public void Show(string text)
        {
            this.text = text;
            Show();
        }


        //Returns value when 'OK' pressed.
        private void ReceiveResult(string result)
        {
            if (saveValue)
            {
                text = result;
                SavePrefs();
            }

            if (OnResult != null)
                OnResult.Invoke(result);
        }
    }
}
