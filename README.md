# TJAPlayer3-f
TJAPlayer3をForkして、趣味程度に改造してます。  
スパゲッティコードと化してますね。うん。たまに整理しましょうかね。

実装してほしいものがあればGitHubのIssuesまたはDiscord鯖まで。  
趣味程度の改造なので時間はかかりますが、実装要望があったものは、なるべく実装したいと考えています。  
(Issueについて、確認済みのものはラベルを貼ります。)  
趣味ですから、気分次第で実装をするので、バグ報告がなされても後回しにする可能性があります。すみません。

masterブランチでほぼすべての開発を行います。  
(基本的なものはです。大規模なテスト実装などは、別のブランチに移行するかも。)

起動時にコンソールが出現しますが、気にしないでください。

osx-x64ビルドはテスト段階です。  
動作確認ができていません。そのため、動作保証ができません。ご承知おきください。

このプログラムを使用し発生した、いかなる不具合・損失に対しても、一切の責任を負いません。


## 推奨環境
* Windows 7以降のWindows環境  
* Ubuntu系のLinux環境 (x64)  

~~まぁ、Windows 10で動作確認をしているので、Windows 10が一番安定してるかと...~~  
__Windows10(Ver.20H2)とLinux Mint 20.1(Xfce)ではLinux Mintの方が安定して動作していました。__


Ubuntu系Linuxではターミナルで  
``sudo apt install freeglut3 freeglut3-dev libgdiplus ffmpeg``  
を実行しそれぞれのライブラリをインストールしてから実行してください。


## 開発環境(動作確認環境)
OS
* Windows 10(Ver.20H2) (x64)  
* Linux Mint 20.1(Xfce) (x64)  
* Ubuntu 20.04 LTS (x64)

Editor
* Visual Studio Community 2019  
* Visual Studio Code  
* Vim

## バグ報告のお願い
改造者:[@Mr_Ojii](https://twitter.com/Mr_Ojii)はC#を2020年3月16日に初めて触りました。  
この改造をしながら、C#を勉強しているため、相当な量のバグが含まれていると思われます。  
バグを見つけた場合、Discordサーバーまたは、Issuesで報告してもらえると、自分の勉強もはかどるのでよろしくお願いします。  
プログラムが落ちるようなエラーである場合、情報を開発者に送信するような仕様になっております。ご了承ください。

## 開発状況(ログみたいなもん)
|バージョン |日付(JST) |                                        実装内容                                        |
|:---------:|:--------:|:---------------------------------------------------------------------------------------|
|Ver.1.5.8.0|2020-03-25|より本家っぽく。                                                                        |
|Ver.1.5.8.1|2020-04-16|王冠機能の搭載(かんたん～おに & Edit(実質裏鬼))                                         |
|Ver.1.5.8.2|2020-04-17|.NET Framework 4.0にフレームワークをアップデート                                        |
|Ver.1.5.8.3|2020-05-06|譜面分岐について・JPOSSCROLLの連打についての既知のバグを修正                            |
|Ver.1.5.9.0|2020-05-08|複数の文字コードに対応                                                                  |
|Ver.1.5.9.1|2020-05-09|WASAPI共有に対応                                                                        |
|Ver.1.5.9.2|2020-05-12|.NET Framework 4.8にフレームワークをアップデート                                        |
|Ver.1.5.9.3|2020-05-22|スコアが保存されないバグを修正 & songs.dbを軽量化                                       |
|Ver.1.6.0.0|2020-06-04|難易度選択画面＆メンテモード追加(タイトル画面でCtrl+Aを押しながら、演奏ゲームを選択)    |
|Ver.1.6.0.1|2020-06-07|Open Taiko Chartへの対応(β)                                                            |
|Ver.1.6.0.2|2020-06-15|片開き(仮)実装                                                                          |
|Ver.1.6.0.3|2020-07-11|特訓モード(仮)実装                                                                      |
|Ver.1.6.0.4|2020-08-30|音色機能の実装・演奏オプション表示方法の変更                                            |
|Ver.1.6.0.5|2020-09-03|FFmpeg APIを使用しての音声デコード機能を追加                                            |
|Ver.1.6.1.0|2020-09-13|FFmpeg APIを使用しての動画デコード機能を追加                                            |
|Ver.1.6.2.0|2020-10-06|.NET Core 3.1にフレームワークをアップデート                                             |
|Ver.1.6.3.0|2021-01-03|.NET 5にフレームワークをアップデート                                                    |
|Ver.1.6.4.0|2021-01-06|OpenGL描画に対応                                                                        |
|Ver.1.7.0.0|2021-03-16|Ubuntu系のLinux Distributionに対応                                                      |
|Ver.1.7.1.0|2021-03-22|描画バグの修正                                                                          |
|Ver.1.7.1.1|2021-03-31|スクリーンショットのバグを修正                                                          |
|Ver.1.7.1.2|2021-04-19|Linux環境でMidi入力ができない問題の修正                                                 |
|Ver.1.7.1.3|2021-04-22|動画再生時のメモリ使用量の変動が大きい問題の修正(また、動画再生時の負荷軽減)            |

## Discord鯖
作っていいものかと思いながら、公開鯖を作ってみたかったので作ってしまいました。  
参加した場合、#readmeをご一読ください。よろしくお願いいたします。  
[https://discord.gg/WtdsBqESaX](https://discord.gg/WtdsBqESaX)

## 追加命令について
Testフォルダ内の「追加・変更機能について.md」で説明いたします。

## 謝辞
このTJAPlayer3-fのもととなるソフトウェアを作成・メンテナンスしてきた中でも  
有名な方々に感謝の意を表し、お名前を上げさせていただきたいと思います。

- FROM/yyagi様
- kairera0467様
- AioiLight様

また、他のTJAPlayer関係のソースコードを参考にさせてもらっている箇所があります。  
ありがとうございます。

## ライセンス関係
Fork元より使用しているライブラリ
* [bass](https://www.un4seen.com/bass.html)
* [Bass.Net](http://bass.radio42.com/)
* FDK21(改造しているので、FDKとは呼べないライブラリと化しています)

以下のライブラリを追加いたしました。
* [ReadJEnc](https://github.com/hnx8/ReadJEnc)
* [Json.NET](https://www.newtonsoft.com/json)
* [FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen)
* [FFmpeg](https://ffmpeg.org/)
* [OpenTK](https://opentk.net/)
* [OpenAL Soft](https://openal-soft.org/)
* [discord-rpc-csharp](https://github.com/Lachee/discord-rpc-csharp)
* [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
* [SixLabors.Fonts](https://github.com/SixLabors/Fonts)
* [M+ FONTS](https://osdn.net/projects/mplus-fonts/)
* [managed-midi](https://github.com/atsushieno/managed-midi)

また、フレームワークに[.NET](https://dotnet.microsoft.com/)を使用しています。

ライセンスは「Test/Licenses」に追加いたしました。

## FFmpegについて
このリポジトリにはあらかじめFFmpegライブラリが同梱されています。  
同梱しているライブラリは
+ [Zeranoe FFmpeg](http://ffmpeg.zeranoe.com/builds/)からのx86ライブラリ  
+ [FFmpeg Static Auto-Builds](https://github.com/BtbN/FFmpeg-Builds)からのx64ライブラリ  

バージョンは4.3.1です。(2021/03/16現在)

DLL群のバージョンアップをしたい方は自己責任で差し替えをしてください。  

## OpenAL Softについて
このリポジトリにはあらかじめOpenAL Softライブラリが同梱されています。  
バージョンは1.21.1です。(2021/03/16現在)

このプログラムはFROM氏の「DTXMania」を元に製作しています。

