Kikisen-VC (音声認識型音声合成VCツール)
====

Overview
text-to-speech, speech-to-text, auto recognized translator system, i wish.

ゲーム用VCツールです。

音声認識を行い、以下の機能を実現することを目指します。

- 音声合成しての発声代替機能
- ループバック音声認識からの翻訳発声機能

## Description
以下に各機能の概要、開発経緯を書きます。

- 音声合成しての発声代替機能
VCは環境音が混ざるのが嫌、なにより気恥ずかしい、そんな思いから開発が始まりました。

外部仮想サウンドデバイス作成ソフトの導入が必要ですが、合成音声で混じりっ気なしの合成音声によるVCが実現できます。

- ループバック音声認識からの翻訳発声機能
海外サーバーでゲームをしているとVCが活発で驚きます。

ただ何を言ってるのかさっぱり聞き取れません。

せっかく音声認識ソフトを作ったのだから、英語の聞き取り→翻訳→日本語で発声まで実現できないかなと思って実装してみました。

ですが、いまのところ使い物にならない実験的な機能です。

独りで開発していては手に負えないと思ったので、GitHubにソースを公開して誰かが助けてくれるのを期待しています。

## Demo
![実行イメージ](https://github.com/zufall-upon/kikisen-vc/blob/master/imgaes/mainimage.PNG)

## Requirement
- nugetで必要なものを書く
naudio

google apis

windowsの英語音声認識エンジン

.net framework

- 使う上で必要なものを書く
Google Cloud Speech API json key

Google Translator API web key

キーの取得方法など書く

## Usage
設定方法とか書く

## Install
- 音声合成しての発声代替機能

必要なツールと設定方法を書く

- ループバック音声認識からの翻訳発声機能

必要な設定を書く

## Licence
使用は自由ですが、改変しての再配布はソースを開示してください。

GNU General Public License v3.0

# New Features!
- MS音声認識のデフォルト化→実質使い物になりません。トレーニングしてのテストはしていません。
