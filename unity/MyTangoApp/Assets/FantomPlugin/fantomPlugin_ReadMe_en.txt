http://fantom1x.blog130.fc2.com/blog-entry-273.html
Android Native Dialogs and Functions Plugin
Setup & Build Manual

･Native Plugin "fantomPlugin.aar" is required 'Minimum API Level：Android 4.2 (API 17)' or later.

･Move the "Assets/FantomPlugin/Plugins/" folder just under "Assets/" like "Assets/Plugins/". This "Plugins" folder is a special folder for running the plugin at runtime.
(see) https://docs.unity3d.com/Manual/ScriptCompileOrderFolders.html

･Rename "AndroidManifest-FullPlugin~.xml" to "AndroidManifest.xml" when receive events of Hardware Volume buttons or displaying dialog by Speech Recognizer.

(*) Recording permission "RECORD_AUDIO" is required to use Speech Recognizer.
(see) https://developer.android.com/reference/android/Manifest.permission.html#RECORD_AUDIO

･Text To Speech is required the reading engine and voice data must be installed on the smartphone.
(see) http://fantom1x.blog130.fc2.com/blog-entry-275.html#fantomPlugin_TextToSpeech_install
(voice data: Google Play)
https://play.google.com/store/apps/details?id=com.google.android.tts
https://play.google.com/store/apps/details?id=jp.kddilabs.n2tts

･Select "_Landscape" or "_Portrait" of "AndroidManifest~.xml" according to the screen rotation attribute (screenOrientation) of the application.
(see) https://developer.android.com/guide/topics/manifest/activity-element.html#screen

･Rename "AndroidManifest_demo.xml" to "AndroidManifest.xml" when building the Demo. Also add the scene in "Assets/_Test/Scenes/" to 'Build Settings...' and switch to 'Android' with 'Switch Platform'.

(*) Warning "Unable to find unity activity in manifest. You need to make sure orientation attribute is set to sensorPortrait manually." can be ignored if you use anything other than Unity standard Activity (UnityPlayerActivity).
(see) https://docs.unity3d.com/ja/current/Manual/AndroidUnityPlayerActivity.html


Let's enjoy creative life!

------------------------------------------------
■Update history

·Added PinchInput, SwipeInput, LongClickInput/LongClickEventTrigger and its demo scene (PinchSwipeTest).
·Added SmoothFollow3 (originally StandardAssets SmoothFollow) with right/left rotation angle, height and distance, and added a corresponding to pinch (PinchInput) and swipe (SwipeInput) (demo scene: used with PinchSwipeTest).
·Changed the color format conversion of 'XColor' from ColorUtility to calculation formulas(Mathf.RoundToInt()).
･Changed 'XDebug' option of lines limit.
･Added prefab and '-Controller' script of all functions.
･Added value change callbacks to SingleChoiceDialog, MultiChoiceDialog, SwitchDialog and CustomDialog items.
･Fixed bug that XDebug's automatic newline flag (newline) was ignored. Also, cleared the text buffer (Queue) with OnDestory() when using line limit.

(*)The latest version can be downloaded from GoogleDrive on blog (Japanese version only).
http://fantom1x.blog130.fc2.com/blog-entry-273.html

------------------------------------------------
■News!

The music library including sample song is on sale at the Asset Store!

Seamless Loop and Short Music (FREE)
https://www.assetstore.unity3d.com/#!/content/107732

------------------------------------------------
By Fantom

[Blog] http://fantom1x.blog130.fc2.com/
[Twitter] https://twitter.com/fantom_1x
[SoundCloud] https://soundcloud.com/user-751508071
[Picotune] http://picotune.me/?@Fantom
[Monappy] https://monappy.jp/u/Fantom

