using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Extensions.CommandLineUtils;

using NLog;

using PGReliefMoreForGit.Models.Update;

namespace PGReliefMoreForGit
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        [STAThread()]
        public static void Main(string[] args)
        {
            // タイトルを設定
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var assm = Assembly.GetExecutingAssembly();
            var name = assm.GetName();
            logger.Info($"----- Start Application ----- {name.Name} [{fvi.ProductVersion} ({name.Version})] -----");

            // プログラム引数の互換性確保のための処理
            EnsureCompatibility(args);


            // プログラム引数の解析
            var cla = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                // アプリケーション名（ヘルプの出力で使用される）
                Name = name.Name,
            };

            // ヘルプ出力のトリガーとなるオプションを指定
            cla.HelpOption("-?|-h|--help");

            // Pre-release をバージョンアップ対象とするオプションを指定
            var preOptions = cla.Option("--pre", "Check pre-release version", CommandOptionType.NoValue);

            // 読み込むファイル
            var inputfileOptions = cla.Option("-f|--file <filename>", "Read a setting file.", CommandOptionType.SingleValue);

            // CLI モード
            cla.Command("nogui", command =>
            {
                // 説明（ヘルプの出力で使用される）
                command.Description = "Command Line Interface Mode (No GUI)";

                command.HelpOption("-?|-h|--help");

                command.OnExecute(() =>
                {
                    Console.WriteLine("Hello World!");
                    return 0;
                });
            });

            // アップデート後のクリーンアップ処理
            cla.Command("up", command =>
            {
                // 説明（ヘルプの出力で使用される）
                command.Description = "Cleanup after update.";

                command.HelpOption("-?|-h|--help");

                // アップデート前のプロセスID
                var pidOptions = command.Option("--pid", "Process ID", CommandOptionType.SingleValue);

                // 削除するファイル
                var deleteFileOptions = command.Option("--file", "Delete files", CommandOptionType.MultipleValue);

                command.OnExecute(() =>
                {
                    Update.Instance.CleanUp(Convert.ToInt32(pidOptions.Value()), deleteFileOptions.Values);
                    Console.WriteLine("cleanup!");
                    return 0;
                });
            });

            // デフォルトの動作
            cla.OnExecute(() =>
            {
                if (preOptions.HasValue())
                {
                    Update.Instance.PreRelease = true;
                }

                // GUI を起動
                App app = new App();
                app.InitializeComponent();
                return app.Run();
            });

            cla.Execute(args);
        }

        static void EnsureCompatibility(string[] args)
        {
            // プロセス ID 置換
            int indexOfUp = Array.IndexOf(args, "/up");
            if (indexOfUp != -1)
            {
                args[indexOfUp] = "up";
                // プログラム引数 /up の次がプロセスID
                args[indexOfUp + 1] = "--pid=" + args[indexOfUp + 1];
            }

            // 削除するファイル置換
            int indexOfDeleteFiles = Array.IndexOf(args, "/deleteFiles");
            if (indexOfDeleteFiles != -1)
            {
                for (int i = indexOfDeleteFiles + 1; i < args.Length; i++)
                {
                    args[i] = "--file=" + args[i];
                }
            }
        }
    }
}

