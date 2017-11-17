using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

using PGReliefMoreForGit.Models.Git;
using PGReliefMoreForGit.Models.PGRelief;

namespace PGReliefMoreForGit.Models
{
	class Analysis
	{
		Logger logger = LogManager.GetCurrentClassLogger();

		GitObj Git = new GitObj();
		List<ChangedLine> ChangedLines = new List<ChangedLine>();

		PGReliefObj PGRelief = new PGReliefObj();
		List<RemarkMessage> RemarkMessages = new List<RemarkMessage>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Analysis() { }

		/// <summary>
		/// 解析を行う
		/// </summary>
		/// <param name="gitRepositoryDirectory"></param>
		/// <param name="pgReliefHtmlFile"></param>
		/// <param name="outputFile"></param>
		public void Run(string gitRepositoryDirectory, string shaHash, string pgReliefHtmlFile, string outputFile)
		{
			// PGRelief の html ファイルを解析し、指摘メッセージを抽出する
			logger.Info("Start reading a PGRelief output file.");
			RemarkMessages.Clear();
			PGRelief.Analysis(pgReliefHtmlFile, RemarkMessages);

			// Git リポジトリの情報から変更範囲を取得する
			logger.Info("Get changed lines from git repository.");
			ChangedLines.Clear();
			Git.ExtractChangedLine(gitRepositoryDirectory, shaHash, ChangedLines);

			// PGRelief の指摘メッセージのうち、変更範囲に含まれているものだけにフィルタリングする
			logger.Info("Filter out PGRelief remarked message.");
			Filtering(ChangedLines, RemarkMessages);

			// フィルタリングした結果を html を出力
			logger.Info("Output html file.");
			PGRelief.OutputHtml(RemarkMessages, outputFile);

			logger.Info("Done.");
		}

		/// <summary>
		/// PGRelief の指摘メッセージのうち、変更範囲に含まれているものだけにフィルタリングする
		/// </summary>
		/// <param name="changedLines"></param>
		/// <param name="remarkMessages"></param>
		void Filtering(List<ChangedLine> changedLines, List<RemarkMessage> remarkMessages)
		{
			remarkMessages.RemoveAll(x =>
			{
				int line = 0;

				if (int.TryParse(x.LineNumber, out line) == true)
				{
					// 数値に変換できるときのみ処理する
					foreach (var changedLine in changedLines)
					{
						if (x.FileName.EndsWith(changedLine.NewPath) == true)
						{
							if ((changedLine.StartLine <= line) &&
								(line < (changedLine.StartLine + changedLine.NumberOfChangedLines)))
							{
								// 変更範囲に含まれている
								return false;
							}
						}
					}

					// 変更範囲に含まれていなかったら削除する
					return true;
				}
				else
				{
					return false;
				}

			});
		}
	}
}
