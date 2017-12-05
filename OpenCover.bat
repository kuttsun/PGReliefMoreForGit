rem ソリューションルートで実行すること

rem MSTEST のインストール先
set MSTEST="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"

rem OpenCover のインストール先
set OPENCOVER="packages\OpenCover.4.6.519\tools\OpenCover.Console.exe"

rem 実行するテストのアセンブリ
set TARGET_TEST="PGReliefMoreForGit.Tests.dll"

rem 実行するテストのアセンブリの格納先
set TARGET_DIR=".\PGReliefMoreForGit.Tests\bin\Release"

rem OpenCover の出力ファイル
set OUTPUT="result.xml"

rem カバレッジ計測対象の指定
rem set FILTERS="+[OpenCoverSample]*"
set FILTERS="+[PGReliefMoreForGit]* -[PGReliefMoreForGit]PGReliefMoreForGit.Properties.* -[PGReliefMoreForGit]PGReliefMoreForGit.ViewModels.* -[PGReliefMoreForGit]PGReliefMoreForGit.Views.*"

rem OpenCover の実行
%OPENCOVER% -register:user -target:%MSTEST% -targetargs:%target_test% -targetdir:%TARGET_DIR% -filter:%FILTERS% -output:%OUTPUT%
