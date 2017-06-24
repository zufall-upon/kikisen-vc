Kikisen-VC (音声認識・音声合成・翻訳・擬似VCツール)
====
Overview
> Text to speech, speech to text, auto-recognized translation system.
> I want it in this software.

![イメージ](http://i.imgur.com/dbNpvcK.png)

**普段聞き専の方向けの汎用擬似VCツール**です。ゲーム等で使用できます。

各種APIで音声認識を行い、以下の機能を実現することを目指します。

1. 機械音声を合成しての**発声代替機能**
 - text-to-speechのAPIを利用して多彩な音声・カスタマイズを提供
2. ループバックサウンドから音声認識しての**翻訳聴き専機能**
 - 英語圏・中華圏等でプレイ中のVC聞き専できる機能を提供

## Description （各機能の概要、開発経緯、特徴）

1. **機械音声を合成しての発声代替機能**

  VCは環境音が混ざるのが嫌、少し気恥ずかしい。でもVCで発言してみたい。それが開発動機でした。外部仮想サウンドデバイス作成ソフトの導入が必要ですが、合成音声で混じりっ気なしの合成音声によるVCが実現できます。

  Windowsならばデフォルトの音声認識APIを利用することもできます。ですが、トレーニングしないととても使い物にならない認識精度です。できれば外部APIを利用するオプションを検討してください。

2. **ループバックサウンドから音声認識しての翻訳聴き専機能**

  海外サーバーでゲームをしているとVCが活発で驚きます。ただ何を言ってるのかさっぱり聞き取れません。せっかく音声認識・合成発声ソフトを作ったのだから、外国語の聞き取り→翻訳→日本語で発声まで実現できないかなと思って実装してみました。

  ですが、いまのところ使い物にならない実験的な機能です。このままひとりで開発していても実用的な機能には至らないと思いました。GitHubにソフトを公開することで、一人でも多くの人がコンセプトに興味を持ってくれ、開発に参加を検討してくれることを望みます。

##### 利点
  - GoogleCloudSpeechAPIを利用した場合は高い認識率
  - VoiceTextWebAPIを利用した場合は多彩な機械音声
  - 発声代替機能は、マイク代替なのでVCが使える全てのゲームで動作可能
  - 雑音や咀嚼音、ため息、キーボード音や環境音など入らない純粋な発声代替機能
  - 聞いた英語の日本語訳がすぐわかるので英語学習能力もUpするかもしれません。将来的に。

##### 欠点
  - クラウドAPIを使う場合、回線状況によっては**pingに影響を及ぼす**
  - NetduettoやVirtual Audio Cables等の**仮想サウンド作成デバイスの併用が必要**

## 類似ソフト
  1. 同様コンセプト系ソフト
    - [NAMAROID](http://ch.nicovideo.jp/StackGamesEmpire/blomaga/ar944481)
    - [ゆかりねっと](http://www.okayulu.moe/)
  2. ソフト組み合わせで実現系、似たようなコンセプト
    - [【棒読みちゃん】LINEやSkypeの通話でゆっくりボイスを使ってみた](https://nijipi.com/it-news/post-578/)
    - [棒読みちゃんをマイク代わりに使う](http://raipc.livedoor.biz/archives/51936570.html)
    - [Windows標準の音声認識ソフトでテキストを入力する方法【マイク】](http://aviutl.info/windows-onnseininnsiki/)
    - [各種チャット用 音声入力Toolによる作業しながらのゆっくり生放送に挑戦してみる（ゆっくり増産計画）](http://ch.nicovideo.jp/yu-kuri-radio/blomaga/ar555046)
    - [SERIFU -script text to speech tool-](https://kanae.net/secondlife/serifu_ja.html)
    - [棒読みちゃんプラグイン Voicetext Talk Ver1.5.0.0](http://ch.nicovideo.jp/Wangdora/blomaga/ar612584)

## Licence
  使用は自由ですが、改変しての再配布はソースを開示してください。

## Requirement
  - 開発環境
    - VisualStudio
    - Nugetで以下の導入が必要。
      - Google.Cloud.Speech.V1
      - Google.Apis.Translate.v2
      - System.Net.Http
      - NAudio
  - 使用環境
    - .Net Framework 4以上
    - 「Windows音声認識API」で英語認識をさせる場合は、Windowsの英語音声認識エンジンの追加導入が必要。
      - 「en-US」を言語に追加してください。[Microsoft Forum](https://social.msdn.microsoft.com/Forums/lync/en-US/032c4abc-5614-49ca-b696-17ab64525538/speechrecognitionengine?forum=csharpgeneralja)
  - オプションを使うのに必要なもの
    - Google Cloud Speech API json key
      - [Google Cloud Speech API - サービスアカウントキー(JSONファイル)発行方法例](https://blog.spot-corp.com/ai/2016/07/28/cloud_speech_api.html)
    - Google Translator API web key
      - [Google翻訳APIキー取得方法](https://cloudapplications.desk.com/customer/ja/portal/articles/2230196-google%E7%BF%BB%E8%A8%B3api%E3%82%AD%E3%83%BC%E5%8F%96%E5%BE%97%E6%96%B9%E6%B3%95)
    - [HOYA VoiceTextWebAPIのAPIキー](https://cloud.voicetext.jp/webapi)
    - 仮想サウンドデバイス作成ソフト
      - [Virtual Audio Cables](http://vb-audio.pagesperso-orange.fr/Cable/index.htm)
      - [Bing Speech API](https://azure.microsoft.com/ja-jp/services/cognitive-services/speech/)
        - 「試用版」をクリックして登録する メアドだけでいい模様
          - ![イメージ](http://i.imgur.com/RLM7dii.png)
        - キー１を使用してください。
          - ![イメージ](http://i.imgur.com/K2vdXyD.png)




## Usage
  - **発声代替機能**の使い方の設定例等
    1. 仮想サウンドデバイス作成ソフトをインストールして、**録音側**を「既定のデバイス」に設定します。
      - ![イメージ](http://i.imgur.com/yjhCim5.png)
      - 仮想サウンドデバイスを「既定のデバイス」に設定しておけば、無設定で機械音声だけをマイク出力させることができます。自分の声は相手に伝わりません。
    1. 「Inputデバイス」に使用中のマイクを指定します。
    1. 「Outputデバイス」に仮想サウンドデバイスを指定します。
      - ![イメージ](http://i.imgur.com/UMPgiPAm.png) ![イメージ](http://i.imgur.com/QJW1clEm.png)
    1. 「音声認識API」「SpeechAPI」を適当に選んで設定します。Microsoft Harukaを使うとPingへの影響が軽減されるかもしれません。
    1. 「翻訳API」を「翻訳なし」に設定します。「翻訳設定」を「Jpn→Eng」に設定します。
      - ![イメージ](http://i.imgur.com/XIrbQyl.png) ![イメージ](http://i.imgur.com/V3qCcMS.png)
    1. 必要に応じて「単語辞書」に発声単語を登録しておきます。
    1. マイクに喋ります。選択した「SpeechAPI」を使って発声が行われます。（イメージ）
      - ![イメージ](http://i.imgur.com/A5GM4oKm.png) ![イメージ](http://i.imgur.com/VQnTQkQm.png)


  - **翻訳聴き専機能**の使い方の設定例等
    1. 「Inputデバイス」に「Wasapi Loopback」を指定します。
      - ![イメージ](http://i.imgur.com/pgGZR8d.png)
    1. **※要課題**「Outputデバイス」にお使いのスピーカー・ヘッドセットを選択します。現在プログラムの問題で、お使いのデバイス以外のものを選べる場合はそれを指定してください。（イヤホンなど併用できる場合はそれを使うなど）
      - ![イメージ](http://i.imgur.com/Za5w2ZL.png)
    1. 「音声認識API」「SpeechAPI」を適当に選んで設定します。Microsoft Harukaを使うとPingへの影響が軽減されるかもしれません。
    1. 「翻訳API」「API key」にGoogleTranslatorAPIのWebkeyを入力して認証します。
      - ![イメージ](http://i.imgur.com/n8S8fwp.png)
    1. 「翻訳API」を「GoogleTranslatorAPI」に設定します。「翻訳設定」を「Eng→Jpn」に設定します。
      - ![イメージ](http://i.imgur.com/SzR7oU8.png) ![イメージ](http://i.imgur.com/T1AtTY3.png)
    1. 「翻訳設定」を「Eng→Jpn」に設定して、「翻訳API」を「翻訳なし」にすると英語のまま認識を出力されるかもしれません。

## New Features!
  - 2017/06/17 初回公開。
  - 2017/06/24 BingSpeechAPIの暫定追加。GoogleCloudSpeechAPIと比べて大声で話す必要がある。辞書機能はまだ対応していません。

## Problems

##### 課題
  - 翻訳聞き専機能はループバックサウンドをキャプチャするため、翻訳発声も拾ってしまう。
    - 英語→日本語の場合でも、カナ英語など混じるとVCと勘違いして二重に翻訳が動く。
    - 対策としては翻訳発声のサウンド出力を既定出力サウンドデバイスと違うものに変える必要があるが、通常はヘッドセット一本で使用していると思われるため、現実的ではない。
  - GoogleCloudSpeechAPIや、GoogleTranslatorAPIなどの利用は有料。API呼び出し回数はできるだけ抑える必要があるが、レスポンスを高めるためストリーミング音声認識APIを利用すると、1分あたり1回のAPI呼び出しが必須。時間課金と変わらない。
  - GoogleTranslatorAPIも課題は山積み。API呼び出し回数を抑えるためには出来るだけ高い精度で文脈の区切りを見つけて翻訳APIに投げる処理が必要。現在はその処理が甘く、二重三重呼び出し・区切りが甘い等でとんでもない回数APIを呼び出してしまう。1分で100回くらい。。
  - 音声認識APIはAPIによって違う処理の実装が必要。さらに言語によっての最適化が必要。その最適化処理がまだまだ甘い。
  - ~~音声認識APIにIntel Real Senseを選べるようにしたい。ローカルエンジンなので、Ping影響なしに使える利点がある。~~
    - Intel Realsense SDKは2016R3から有料配布になった模様。普通の方法ではDLできないので中止。
    - **代わりにBing Speech APIを使いたい。月5000トランザクションまで無料なので、1トランザクション14秒制限はあるが、単純計算で14sec * 5000tra = 70000sec, 月19.4時間までは無料で使えそうな勘定。1回2時間のゲームセッションを月10回は聞き専としては妥当だと思う。**
  - 翻訳APIに無料のものがあれば使いたい。有料しかなさげ。
  - GoogleCloudSpeechAPIは、2017/7/31までにGA版にアップデートが必要。まだ対応できていない。


## Samples
![イメージ](http://i.imgur.com/dbNpvcK.png)


  - 音声入出力デバイス設定関係（設定詳細はUsageを参照のこと）
    - ![イメージ](http://i.imgur.com/x7bVKns.png) ![イメージ](http://i.imgur.com/i3D0954.png)


  - 音声認識API設定関係（認証JSONファイルを指定してください）
    - ![イメージ](http://i.imgur.com/Jw5EfWF.png) ![イメージ](http://i.imgur.com/XtmVPu3.png)


  - VoiceTextWebAPIを利用した場合(ここでAPIkey設定、ログ出力もここ)
    - ![イメージ](http://i.imgur.com/dt5NdaQ.png) ![イメージ](http://i.imgur.com/YSpi5Cv.png) ![イメージ](http://i.imgur.com/8A5t3ho.png) ![イメージ](http://i.imgur.com/QwcCQQu.png)


  - 翻訳設定関係(ここで設定)
    - ![イメージ](http://i.imgur.com/XIrbQyl.png) ![イメージ](http://i.imgur.com/V3qCcMS.png) ![イメージ](http://i.imgur.com/zrdtu1s.png)


  - ステータス関係（ここで大体の状況を認識）
    - ![イメージ](http://i.imgur.com/app9Kws.png)


  - 単語辞書機能（入力すると自動保存されます、「|」パイプ区切りで入力）
    - ![イメージ](http://i.imgur.com/kacG23S.png)


  - 音声認識ログサンプル(GoogleCloudSpeechAPI使用の場合)
    - 日本語でのマイク入力→発声代替機能ログ
      - ![イメージ](http://i.imgur.com/fpp4UVR.png) ![イメージ](http://i.imgur.com/vCMcuVJ.png)


  - 英語でのループバック入力→翻訳聴き専機能
    - 開発版での英語のまま音声認識したイメージ(Insurgencyの海外鯖プレイ動画を認識)
      - ![イメージ](http://i.imgur.com/adUBAfi.png)

    - 日本語への翻訳イメージ(Insurgencyの海外鯖プレイ動画を認識)
      - ![イメージ](http://i.imgur.com/GbFnzgx.png)

    - 日本語への翻訳イメージ(「家出のドリッピー」朗読動画を認識)
      - ![イメージ](http://i.imgur.com/tHxZeKV.png)
