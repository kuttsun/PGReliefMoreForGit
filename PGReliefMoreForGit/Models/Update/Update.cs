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

namespace PGReliefMoreForGit.Models.Update
{
	public class Update : NotificationObject
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

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Update(string githubReleaseURL)
		{
			gitHubReleaseURL = githubReleaseURL;
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
					webClient.DownloadFile($"{gitHubReleaseURL}download/{latestVersion}/{fileName}", fileName);
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
							Console.WriteLine("リネームして展開: " + entry.FullName);
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
						Console.WriteLine("展開成功: " + entry.FullName);
					}
					catch (Exception e)
					{
						Console.WriteLine("展開失敗: " + e.Message);
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
			// 最新バージョンの zip ファイルをダウンロードする（ダウンロード前に今ある zip ファイルは削除しておく）
			File.Delete(archiveFileName);
			DownloadLatestVersion(archiveFileName);

			// 現在のファイル一式をアーカイブして取っておく
			ArchiveDir(Directory.GetCurrentDirectory(), backupFileName);

			// ダウンロードした zip ファイルを展開し、１ファイルずつ上書きしていく
			ExtractEntries(archiveFileName);

			// アプリケーションの再起動
			Process.Start(exeFullName, "/up " + Process.GetCurrentProcess().Id);

			return true;
		}

		/// <summary>
		/// アップデート後の後始末を行う
		/// </summary>
		/// <returns></returns>
		public bool CleanUp()
		{
			// プログラム引数 /up が指定されているインデックスを取得
			int index = Environment.CommandLine.IndexOf("/up");
			if (index != -1)
			{
				try
				{
					// プログラム引数を取得
					string[] args = Environment.GetCommandLineArgs();
					// プログラム引数 /up の次がプロセスID
					int pid = Convert.ToInt32(args[index + 1]);
					Console.WriteLine("id=" + pid);
					// 終了待ち
					Process.GetProcessById(pid).WaitForExit();
				}
				catch (Exception)
				{
				}

				// プログラムが終了したので古いファイルを削除
				File.Delete(oldExeFileName);
				Console.WriteLine("Update Completed");
				return true;
			}

			return false;
		}
	}
}
