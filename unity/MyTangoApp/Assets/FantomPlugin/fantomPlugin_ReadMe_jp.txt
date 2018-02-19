http://fantom1x.blog130.fc2.com/blog-entry-273.html
Android Native Dialogs and Functions Plugin
セットアップ＆ビルド マニュアル

･ネイティブプラグイン "fantomPlugin.aar" は「Minimum API Level：Android 4.2 (API 17)」以上で使用して下さい。

･"Assets/FantomPlugin/Plugins/" フォルダを "Assets/Plugins/" のように "Assets/" 直下に移動して下さい。この「Plugins」フォルダはランタイムでプラグインを稼働させるための特殊なフォルダとなります。
(参照) https://docs.unity3d.com/ja/current/Manual/ScriptCompileOrderFolders.html

･ハードウェア音量キーのイベント取得、または音声認識でダイアログを表示する場合には "AndroidManifest-FullPlugin~.xml" を "AndroidManifest.xml" にリネームして使用して下さい。

(※) 音声認識を使用するには録音パーミッション「RECORD_AUDIO」が必要です。
(参照) https://developer.android.com/reference/android/Manifest.permission.html#RECORD_AUDIO

･テキスト読み上げを使用するには、端末に読み上げエンジンと音声データがインストールされている必要があります。
(テキスト読み上げのインストール)
http://fantom1x.blog130.fc2.com/blog-entry-275.html#fantomPlugin_TextToSpeech_install
(音声データ：Google Play)
https://play.google.com/store/apps/details?id=com.google.android.tts
https://play.google.com/store/apps/details?id=jp.kddilabs.n2tts

･"AndroidManifest~.xml"の"_Landscape" または "_Portrait" はアプリの画面回転の属性(screenOrientation)に合わせて選択して下さい。
(参照) https://developer.android.com/guide/topics/manifest/activity-element.html#screen

･デモをビルドするときは "AndroidManifest_demo.xml" を "AndroidManifest.xml" にリネームして使って下さい。また「Build Settings...」に Assets/_Test/Scenes/ にあるシーンを追加して、「Switch Platform」で「Android」にしてビルドして下さい。

(※) 警告「Unable to find unity activity in manifest. You need to make sure orientation attribute is set to sensorPortrait manually.」は Unityの標準のアクティビティ(UnityPlayerActivity)以外を使うと出るので無視して下さい。

それではよりよい作品の手助けになることを、心から願っています。

------------------------------------------------
■更新履歴

・ピンチ（PinchInput）,スワイプ（SwipeInput）,ロングタップ（LongClickInput/LongClickEventTrigger）とそのデモシーン（PinchSwipeTest）を追加。
・SmoothFollow3（元は StandardAssets の SmoothFollow）に左右回転アングルと高さと距離の遠近機能を追加し、ピンチ（PinchInput）やスワイプ（SwipeInput）にも対応させた改造版（SmoothFollow3）を追加（デモシーン：PinchSwipeTest で使用）。
・XColor の色形式変換を ColorUtility から計算式(Mathf.RoundToInt())に変更。
・XDebug に行数制限を追加。
・おおよそ全ての機能のプレファブ＆「～Controller」スクリプトを追加。
・単一選択（SingleChoiceDialog）、複数選択（MultiChoiceDialog）、スイッチダイアログ（SwitchDialog）、カスタムダイアログのアイテムに値変化のコールバックを追加。
・XDebug の自動改行フラグ(newline)が無視されていた不具合を修正。また、行数制限を使用しているときに、OnDestory() でテキストのバッファ（Queue）をクリアするようにした。

※最新版はブログにて GoogleDrive からダウンロードできます（日本語版のみ）。
http://fantom1x.blog130.fc2.com/blog-entry-273.html

------------------------------------------------
■News!

アセットストアにてサンプルの楽曲を含む音楽ライブラリが公開中！

Seamless Loop and Short Music (FREE)
https://www.assetstore.unity3d.com/#!/content/107732

------------------------------------------------
By Fantom

[Blog] http://fantom1x.blog130.fc2.com/
[Twitter] https://twitter.com/fantom_1x
[SoundCloud] https://soundcloud.com/user-751508071
[Picotune] http://picotune.me/?@Fantom
[Monappy] https://monappy.jp/u/Fantom

