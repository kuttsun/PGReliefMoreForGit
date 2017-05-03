using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

using System.Diagnostics;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using HtmlAgilityPack;
using NLog;

namespace PGReliefMoreForGit.Models.Update
{
	/// <summary>
	/// アプリケーションの自動アップデート機能を提供するクラス（Singleton）
	/// </summary>
	public sealed class Update : NotificationObject
	{
		/*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

		// GitHub のリリースページの URL
		readonly string gitHubReleaseURL = string.Empty;
		// exe ファイルの絶対パス（正確にはアセンブリ名）
		readonly string exeFullName = string.Empty;
		// exe ファイル名
		readonly string exeFileName = string.Empty;
		// リネーム後の古い exe ファイル名
		readonly string oldExeFileName = string.Empty;
		// ダウンロードしてくる zip ファイル名
		readonly string archiveFileName = string.Empty;
		// バックアップした後の zip ファイル名
		readonly string backupFileName = string.Empty;
		// 最新のバージョン
		string latestVersion = string.Empty;

		Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// 自分のインスタンス
		/// </summary>
		public static Update Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Update();
				}
				return instance;
			}
		}
		static Update instance = null;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		private Update()
		{
			gitHubReleaseURL = "https://github.com/kuttsun/PGReliefMoreForGit/releases";
			exeFullName = Assembly.GetExecutingAssembly().Location;
			exeFileName = Path.GetFileName(exeFullName);
			oldExeFileName = Path.GetFileName(exeFullName) + ".old";
			archiveFileName = Path.GetFileNameWithoutExtension(exeFullName) + ".zip";
			backupFileName = "Backup.zip";
		}

		/// <summary>
		/// 最新のアップデートがあるかどうかチェックする
		/// </summary>
		/// <param name="currentVersion"></param>
		/// <returns></returns>
		public bool ExistsUpdate(string currentVersion, out string latestVersion)
		{
			// HtmlAgilityPack では直接URLのデータを取得できないようなので、
			// WebClient を使用して取得する
			using (WebClient webClient = new WebClient())
			using (Stream stream = webClient.OpenRead(gitHubReleaseURL))
			{
				var html = new HtmlDocument();

				// ページを取得
				html.Load(stream);

				// spanタグのやつの中で
				var nodes = html.DocumentNode.Descendants("span")
					// class が css-truncate-target のやつを取得し
					.Where(node => node.GetAttributeValue("class", string.Empty).Contains("css-truncate-target"))
					// タグの中身を取得
					.Select(node => node.InnerHtml);

				// 取得したタグの中身（バージョンのリスト）を順に表示
				foreach (var version in nodes)
				{
					Console.WriteLine(version);
				}

				// バージョンを取得できた
				if (nodes.Count() > 0)
				{
					// 一番最初が最も新しいバージョン
					latestVersion = nodes.First().ToString();

					var version1 = new Version(currentVersion);
					var version2 = new Version(latestVersion);
					if (version1 < version2)
					{
						this.latestVersion = latestVersion;
						return true;
					}
				}
			}

			latestVersion = string.Empty;
			return false;
		}

		/// <summary>
		/// 最新バージョンのファイルをダウンロードする
		/// </summary>
		/// <param name="latestVersion"></param>
		/// <returns></returns>
		bool DownloadLatestVersion(string fileName)
		{
			if (latestVersion != string.Empty)
			{
				using (WebClient webClient = new WebClient())
				{
					var downloadFileName = $"{gitHubReleaseURL}/download/{latestVersion}/{fileName}";
					webClient.DownloadFile(downloadFileName, fileName);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 指定したディレクトリ内のファイルとフォルダをまとめてzipにする
		/// </summary>
		/// <param name="targetDir"></param>
		/// <param name="destinationFileName"></param>
		bool ArchiveDir(string targetDir, string destinationFileName)
		{
			File.Delete(destinationFileName);

			// ファイル一覧を取得
			IEnumerable<string> files = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);

			// 順にzipに追加
			using (var archive = ZipFile.Open(destinationFileName, ZipArchiveMode.Update))
			{
				foreach (string file in files)
				{
					archive.CreateEntryFromFile(file, GetRelativePath(targetDir + '\\', file), CompressionLevel.Optimal);
				}
			}

			return true;
		}

		/// <summary>
		/// 引数１のディレクトリから見た引数２のファイルへの相対パスを取得する
		/// </summary>
		/// <param name="uri1">基準となるディレクトリへの絶対パス(最後は\で終わっている必要あり)</param>
		/// <param name="uri2">目的のファイルへの絶対パス</param>
		/// <returns>引数１のディレクトリから見た引数２のファイルへの相対パス</returns>
		/// <example>
		/// GetRelativePath(@"C:\Windows\System\", @"C:\Windows\file.txt")
		/// ..\file.txt
		/// </example>
		string GetRelativePath(string uri1, string uri2)
		{
			Uri u1 = new Uri(uri1);
			Uri u2 = new Uri(uri2);

			Uri relativeUri = u1.MakeRelativeUri(u2);

			string relativePath = relativeUri.ToString();

			relativePath.Replace('/', '\\');

			return (relativePath);
		}

		/// <summary>
		/// 指定したzipファイルの中身を１つずつカレントディレクトリに展開する
		/// </summary>
		/// <param name="archiveFileName"></param>
		bool ExtractEntries(string archiveFileName)
		{
			// ZIPファイルを開いてZipArchiveオブジェクトを作る
			using (ZipArchive archive = ZipFile.OpenRead(archiveFileName))
			{
				// 選択したファイルを指定したフォルダーに書き出す
				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					try
					{
						if (entry.FullName == exeFileName)
						{
							// exeファイルは直接上書きできないので、リネームして展開し、再起動後に古いファイルを削除する
							File.Delete(oldExeFileName);
							File.Move(exeFileName, oldExeFileName);
						}
						else
						{
							// 展開前に現在のファイルを削除
							File.Delete(entry.FullName);
						}
						// ZipArchiveEntryオブジェクトのExtractToFileメソッドにフルパスを渡す
						entry.ExtractToFile(entry.FullName);
						logger.Info("展開成功: " + entry.FullName);
					}
					catch (Exception e)
					{
						logger.Error("展開失敗: " + e.Message);
					}
				}
			}

			return true;
		}

		/// <summary>
		/// アップデートを実行する
		/// </summary>
		/// <returns></returns>
		public bool RunUpdate()
		{
			logger.Info("アップデート開始");

			// 現在のファイル一式をアーカイブして取っておく
			ArchiveDir(Directory.GetCurrentDirectory(), backupFileName);

			logger.Info("バックアップ完了");

			// 最新バージョンの zip ファイルをダウンロードする（ダウンロード前に今ある zip ファイルは削除しておく）
			File.Delete(archiveFileName);
			DownloadLatestVersion(archiveFileName);

			logger.Info("最新バージョンのダウンロード完了");

			// ダウンロードした zip ファイルを展開し、１ファイルずつ上書きしていく
			ExtractEntries(archiveFileName);

			// ダウンロードした zip ファイルを削除
			File.Delete(archiveFileName);

			logger.Info("ダウンロードファイルの削除完了");

			// アプリケーションの再起動
			string arguments = "/up " + Process.GetCurrentProcess().Id;
			logger.Info($"アプリケーション再起動({arguments})");
			Process.Start(exeFullName, arguments);

			return true;
		}

		/// <summary>
		/// アップデート後の後始末を行う
		/// </summary>
		/// <returns></returns>
		public bool CleanUp()
		{
			// プログラム引数を取得
			string[] args = Environment.GetCommandLineArgs();

			// プログラム引数 /up が指定されているインデックスを取得
			int index = Array.IndexOf(args, "/up");
			if (index != -1)
			{
				logger.Info("アップデート後のクリーンアップ開始");

				try
				{
					logger.Debug($"index={index}");

					foreach (var arg in args)
					{
						logger.Debug($"arg={arg}");
					}

					// プログラム引数 /up の次がプロセスID
					int pid = Convert.ToInt32(args[index + 1]);
					logger.Info("id=" + pid);

					// 終了待ち
					Process.GetProcessById(pid).WaitForExit();

					// プログラムが終了したので古いファイルを削除
					File.Delete(oldExeFileName);
					logger.Info("Update Completed");
					return true;
				}
				catch (Exception e)
				{
					logger.Fatal(e.Message);
				}
			}

			return false;
		}
	}
}
