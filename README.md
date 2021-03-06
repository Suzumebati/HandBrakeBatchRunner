# HandBrake Batch Runnerについて
HandbreakCLIを使って動画ファイルをまとめて変換するアプリです。

Handbreakはとても優秀な動画変換ソフトです。
しかしまとめて変換したいときには少し不便です。
これを解決するためにGUIで指定したものを連続実行することで便利に動画を変換するツールになります。

個人用としてVB.NET+WinFormで作っていましたが、
勉強をしたいので.NET Core+WPFで作り直して公開しようと思います。

# 概要
![operation](https://user-images.githubusercontent.com/51582636/71642448-a331a600-2cee-11ea-9957-fcb2422b36db.gif)

指定の変換設定で、ドラッグアンドドロップしたファイルが連続で変換されます。  
監視フォルダを指定している場合は、ファイルが格納されると変換が自動開始されます。

## 前提環境

Windows 32bit or 64bit

Handbrake Command Lineをインストール(どこかに解凍)されている必要があります。    
以下のHandbrake公式サイトからダウンロードできます。  
https://handbrake.fr/downloads2.php

# 使い方
## 本ソフトのダウンロード方法
リリースページで最新版のバイナリがダウンロードできます。  
以下のリンクからダウンロードしてください。  
リンク：[HandBrake Batch Runner Release](https://github.com/Suzumebati/HandBrakeBatchRunner/releases)

## 使用方法
1. 変換設定を行う。(HandbrakeCLIのコマンドテンプレートを変更します)  
※デフォルトで参考にH265QSVの設定を入れていますが、Handbrake Command Lineのオプションなら設定で自由に変更できます。  
2. ファイルの変換先などを設定する。
3. 変換したいファイルをドラックアンドドロップする。
4. 変換開始をクリックする。

# 便利な機能
- フォルダ監視設定をすると、自動でファイルの変換を行うことができる。
- 別ウインドウで変換中の場合は、後から変換開始したものが待ち合わせすることができる。(多重変換しないようにできる)
- 変換完了フォルダを指定しておくと変換完了したファイルが移動される。
- 既に同名の変換ファイルがあると変換をスキップする。(2重変換防止)
- 次にキャンセルボタンでキャンセルすると変換途中のファイルが完了後にキャンセルすることができる。
- 変換中にでもファイルの追加、削除ができる。
- ファイルリストは保存し、読み込むことで再開することができる。(変換完了しているファイルはスキップ)

# 実装予定
- マルチランゲージ
- マテリアルデザイン
