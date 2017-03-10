using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

using LibGit2Sharp;
using NLog;

namespace PGReliefMoreForGit.Models.Git
{
	class GitObj
	{
		Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public GitObj() { }

		/// <summary>
		/// ワーキングツリーと指定したコミットとの差分情報を抽出する
		/// </summary>
		/// <param name="gitRepositoryDirectory">Git リポジトリのある場所</param>
		/// <param name="ShaHash"></param>
		public void ExtractChangedLine(string gitRepositoryDirectory, string shaHash, List<ChangedLine> changedLines)
		{
			using (var repo = new Repository(gitRepositoryDirectory))
			{
				// リポジトリの中から、指定したSHAハッシュ値のコミットを取得する
				Commit commit = repo.Commits.Where(x => x.Sha == shaHash).First();

				// HEAD と指定したコミットとの差分を取得
				Patch diffs = repo.Diff.Compare<Patch>(commit.Tree, repo.Head.Tip.Tree);

				// 差分情報を順に処理
				foreach (var diff in diffs)
				{
					logger.Trace($"変更前ファイル:{diff.OldPath} 変更後ファイル:{diff.Path}");

					// diff.Patch にユニファイド形式の差分情報が入っているので解析する
					AnalysisUnifiedText(diff, changedLines);
				}
			}
		}

		/// <summary>
		/// ユニファイド形式のテキストを解析する
		/// </summary>
		void AnalysisUnifiedText(PatchEntryChanges diff, List<ChangedLine> changedLine)
		{
			int startLine = 0;
			int numberOfLines = 0;
			int lineCount = 0;

			// 内容の異なる行の開始行
			int startDifferentLine = 0;
			// 内容の異なる行数
			int differentLineCount = 0;

			// ユニファイド形式の差分情報を一行ずつ読み込んで処理する
			using (var rs = new StringReader(diff.Patch))
			{
				// 読み込む文字列がなくなったら -1 が返る
				while (rs.Peek() >= 0)
				{
					// 一行読み込む
					string strline = rs.ReadLine();

					// 先頭が @@ で始まっている場合は変更行に対する情報
					if (strline.StartsWith("@@") == true)
					{
						startLine = 0;
						numberOfLines = 0;
						lineCount = 0;
						startDifferentLine = 0;
						differentLineCount = 0;

						// 正規表現で変更範囲を抽出する
						// ※以下の記事がわかりやすかった
						// http://www.atmarkit.co.jp/fdotnet/dotnettips/579regexmatch/regexmatch.html
						Match match = Regex.Match(strline, @".*@@.+,.+ \+(?<StartLine>.*?),(?<NumberOfLines>.*?) @@");

						if (match.Success == true)
						{
							startLine = int.Parse(match.Groups["StartLine"].Value);
							numberOfLines = int.Parse(match.Groups["NumberOfLines"].Value);
							logger.Trace($"<UnifiedText>ファイル名:{diff.Path} 開始行:{startLine} 行数:{numberOfLines}");
						}
					}
					else
					{
						// 差分情報を取得できている場合にのみ処理
						if (startLine > 0)
						{
							// 先頭が スペース で始まっている場合は変更なしの内容
							// 先頭が + で始まっている場合は変更後のファイルの内容
							// この２つが変更後のファイルに含まれる行なので、この２つだけを行数としてカウントする
							if (strline.StartsWith(" ") == true)
							{
								lineCount++;
							}

							if (strline.StartsWith("+") == true)
							{
								if (startDifferentLine == 0)
								{
									startDifferentLine = startLine + lineCount;
								}
								lineCount++;
								differentLineCount++;
							}
							else
							{
								// 先頭が + でない場合は差分情報を登録する
								if (startDifferentLine > 0)
								{
									AddChangeLine(changedLine, diff.Path, startDifferentLine, differentLineCount);
									startDifferentLine = 0;
									differentLineCount = 0;
								}
							}
						}
					}
				}

				// 未登録の差分情報があるかもしれないので、ここで再度登録
				if (startDifferentLine > 0)
				{
					AddChangeLine(changedLine, diff.Path, startDifferentLine, differentLineCount);
				}
			}
		}

		/// <summary>
		/// 差分情報を登録する
		/// </summary>
		/// <param name="changedLine"></param>
		/// <param name="path"></param>
		/// <param name="startDifferentLine"></param>
		/// <param name="differentLineCount"></param>
		void AddChangeLine(List<ChangedLine> changedLine, string path, int startDifferentLine, int differentLineCount)
		{
			logger.Trace($"<抽出結果>ファイル名:{path} 開始行:{startDifferentLine} 行数:{differentLineCount}");
			changedLine.Add(new ChangedLine(path, startDifferentLine, differentLineCount));
		}
	}
}
