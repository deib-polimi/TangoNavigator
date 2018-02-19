using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using FantomLib;

//Speech Recognizer demo using controllers
public class SpeechRecognizerTest2 : MonoBehaviour {

    public Text displayText;
    public Toggle webSearchToggle;
    public Button recongizerButton;
    public Animator circleAnimator;
    public Animator voiceAnimator;

    public string recognizerStartMessage = "Starting Recognizer...";    //Message when recognizer start.  //音声認識を起動してます…
    public string recognizerReadyMessage = "Waiting voice...";          //Message when recognizer ready.  //音声を待機中…
    public string recognizerBeginMessage = "Recognizing voice...";      //Message when recognizer begin.  //音声を取得しています…

    public bool isRecognitionMultiChoice = true;    //Use 'MultiChoice' for word selection methods (false is 'SingleChoice').

    //Register 'WebSearchController.StartSearch' in the inspector.
    [Serializable] public class WebSearchHandler : UnityEvent<string> { };
    public WebSearchHandler OnWebSearch;

    //Register 'SelectDialogController.Show' etc. in the inspector.
    [Serializable] public class SearchWordSelectHandler : UnityEvent<string[]> { };
    public SearchWordSelectHandler OnSearchWordSelect;

    //Register 'SingleChoiceDialogController.Show' in the inspector.
    [Serializable] public class SingleChoiceHandler : UnityEvent<string[]> { };
    public SingleChoiceHandler OnSingleChoice;

    //Register 'MultiChoiceDialogController.Show' in the inspector.
    [Serializable] public class MultiChoiceHandler : UnityEvent<string[]> { };
    public MultiChoiceHandler OnMultiChoice;


    // Use this for initialization
    private void Start () {
        SpeechRecognizerDialogController speechRecognizer = FindObjectOfType<SpeechRecognizerDialogController>();
        if (speechRecognizer != null)
            DisplayText("isSpeechRecognizerSupported = " + speechRecognizer.IsSupportedRecognizer);
    }
    
    // Update is called once per frame
    //private void Update () {
        
    //}


    //==========================================================
    //Display text string

    //Display text string (and for reading)
    public void DisplayText(string message)
    {
        if (displayText != null)
            displayText.text = message;
    }

    public void DisplayText(string[] words)
    {
        if (displayText != null)
            displayText.text = string.Join("\n", words);
    }


    //Search words by web.
    public void StartWebSearch(string word)
    {
        if (OnWebSearch != null)
            OnWebSearch.Invoke(word);
    }

    //Toggle button (webSearchToggle) to switch WebSearch.
    public void SwitchWebSearch(string[] words)
    {
        if (webSearchToggle != null && webSearchToggle.isOn)
        {
            if (words.Length > 1)
            {
                if (OnSearchWordSelect != null)
                    OnSearchWordSelect.Invoke(words);
            }
            else
                StartWebSearch(words[0]);    //When there is only one word.
        }
        else
        {
            if (words.Length > 1)
            {
                if (isRecognitionMultiChoice)
                {
                    if (OnMultiChoice != null)
                        OnMultiChoice.Invoke(words);
                }
                else
                {
                    if (OnSingleChoice != null)
                        OnSingleChoice.Invoke(words);
                }
            }
            else
                DisplayText(words[0]);    //When there is only one word.
        }
    }


    //==========================================================
    //Example with Google dialog

    //Receive results from speech recognition with dialog.
    public void OnResultSpeechRecognizerDialog(string[] words)
    {
        DisplayText(words);
        SwitchWebSearch(words);
    }


    //==========================================================
    //Example without dialog (Callback handlers)

    //Callback handler for start Speech Recognizer
    public void OnStartRecognizer()
    {
        DisplayText(recognizerStartMessage);
        StartUI();
    }

    //Callback handler for microphone standby state
    public void OnReady()
    {
        DisplayText(recognizerReadyMessage);
        ReadyUI();
    }

    ///Callback handler for microphone begin speech recognization state
    public void OnBegin()
    {
        DisplayText(recognizerBeginMessage);
        BeginUI();
    }

    //Receive the result when speech recognition succeed.
    public void OnResult(string[] words)
    {
        ResetUI();
        DisplayText(words);
        SwitchWebSearch(words);
    }

    //Receive the error when speech recognition fail.
    public void OnError(string message)
    {
        ResetUI();
        DisplayText(message);
    }


    //==========================================================
    //UI

    //Start Recognizer UI
    private void StartUI()
    {
        if (recongizerButton != null)
            recongizerButton.interactable = false;
    }

    //Microphone standby UI
    private void ReadyUI()
    {
        if (circleAnimator != null)
            circleAnimator.SetTrigger("ready");

        if (voiceAnimator != null)
            voiceAnimator.SetTrigger("ready");
    }

    //Microphone begin speech recognization UI
    private void BeginUI()
    {
        if (circleAnimator != null)
            circleAnimator.SetTrigger("speech");

        if (voiceAnimator != null)
            voiceAnimator.SetTrigger("speech");
    }

    //Reset UI
    private void ResetUI()
    {
        if (circleAnimator != null)
            circleAnimator.SetTrigger("stop");

        if (voiceAnimator != null)
            voiceAnimator.SetTrigger("stop");

        if (recongizerButton != null)
            recongizerButton.interactable = true;
    }
}
