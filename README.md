# PGReliefMoreForGit

[![Build status](https://ci.appveyor.com/api/projects/status/jhmgwpxn9af1c4qa?svg=true)](https://ci.appveyor.com/project/kuttsun/pgreliefmoreforgit)

富士通の C/C++ コードの静的解析ツール [PGRelief](http://www.fujitsu.com/jp/group/fst/products/pgr/) の出力結果をフィルタリングするしょぼいアプリケーションです。  
VCS として Git を使用していることが前提です。

Git のリポジトリ情報をもとに、PGRelief の出力結果の中から変更した箇所に対する指摘のみを抽出します。  
変更箇所の判別するためにコミットのハッシュ値を指定します。  
指定したハッシュ値のコミットとワーキングツリーとの差分を変更箇所とみなします。  
尚、PGRelief の指摘メッセージは html で出力する必要があります（PGRelief 2016 以降で対応？）。

- [ダウンロードはこちら](https://github.com/kuttsun/PGReliefMoreForGit/releases)

# 動作環境

- .NET Framework 4.5.1+

# コマンド、オプション

以下のコマンドライン引数に対応しています。

`-?|-h|--help`

ヘルプを表示します。

`-f|--file <PATH>`

起動時に読み込むファイルを指定します。

`--pre`

Pre-release のバージョンを自動アップデート対象とします。

## nogui コマンド (将来対応)

`nogui` を指定することで CLI モードで動作します（GUI を表示しません）。

このコマンドでは以下の引数とオプションに対応しています。

`-f|--file <PATH>`

読み込むファイルを指定します（必須）。

### サンプル

```sh
PGReliefMoreForGit.exe nogui -f=hoge.json
```

## up コマンド

アップデート処理で使用します（ユーザーが直接指定することはありません）。  
このコマンドでは以下の引数とオプションに対応しています。

`--pid <PROCESS_ID>`

アップデート処理を行っているプロセス ID を指定します。

`-f|--file <PATH>`

削除するファイル名を指定します（複数指定可）。


# 開発環境

- Visual Studio 2017
