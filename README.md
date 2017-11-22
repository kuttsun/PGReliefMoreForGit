# PGReliefMore for Git

[![Build status](https://ci.appveyor.com/api/projects/status/jhmgwpxn9af1c4qa?svg=true)](https://ci.appveyor.com/project/kuttsun/pgreliefmoreforgit)



富士通の C/C++ コードの静的解析ツール [PGRelief](http://www.fujitsu.com/jp/group/fst/products/pgr/) の出力結果をフィルタリングするアプリケーションです。  
VCS として Git を使用していることが前提です。  
Git のリポジトリ情報をもとに、PGRelief の出力結果の中から変更した箇所に対する指摘のみを抽出します。  
変更箇所の判別するためにコミットのハッシュ値を指定します。  
指定したハッシュ値のコミットとワーキングツリーとの差分を変更箇所とみなします。  
尚、PGRelief の指摘メッセージは html で出力する必要があります（PGRelief 2016 以降で対応？）。  
設定は `FileSetting.xml` に記載されているものを起動時に読み込みます。

# 動作環境

- .NET Framework 4.5+

# 開発環境

- Visual Studio 2015 Express for Windows Desktop
