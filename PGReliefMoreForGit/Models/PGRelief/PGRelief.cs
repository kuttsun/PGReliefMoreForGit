using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using System.Threading;
using NLog;
using HtmlAgilityPack;// HtmlAgilityPackを使うために必要
					  // using HtmlAgilityPack で曖昧な参照になってコンパイルエラーになる場合は、using hap=HtmlAgilityPack などとして代入しておく

namespace PGReliefMoreForGit.Models.PGRelief
{
	enum PGReliefColumn
	{
		/// <summary>
		/// チェック済み
		/// </summary>
		IsChecked,
		/// <summary>
		/// ファイル名
		/// </summary>
		FileName,
		/// <summary>
		/// 行番号
		/// </summary>
		LineNumber,
		/// <summary>
		/// グループ
		/// </summary>
		Group,
		/// <summary>
		/// 指摘ID
		/// </summary>
		ID,
		/// <summary>
		/// 指摘メッセージ
		/// </summary>
		Message,
		/// <summary>
		/// プロジェクト
		/// </summary>
		Project,
		/// <summary>
		/// 件数
		/// </summary>
		Number,
	};

	class PGReliefObj
	{
		Logger logger = LogManager.GetCurrentClassLogger();
		HtmlDocument html = new HtmlDocument();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public PGReliefObj() { }

		/// <summary>
		/// PGRelief で出力した html ファイルを解析する
		/// </summary>
		/// <param name="htmlFile"></param>
		/// <returns></returns>
		public bool Analysis(string htmlFile, List<RemarkMessage> remarkMessages)
		{
			// HTMLファイルの読み込み
			// ※PGRelief が出力したファイルは Shift-JIS なので、文字コードを指定して読み込む
			string htmlText = File.ReadAllText(htmlFile, Encoding.GetEncoding("Shift_JIS"));

			// 改行コードを取り除く
			htmlText = htmlText.Replace("\r", "").Replace("\n", "");

			// HTMLのテキスト読み込み
			html.LoadHtml(htmlText);

			// 要素の取得
			HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//html");

			// HTMLファイルを解析して PGRelief の指摘メッセージをリスト化する
			ProcessingOnNodes(nodes, remarkMessages);

			return true;
		}

		/// <summary>
		/// 各ノードに対する処理（再帰処理）
		/// </summary>
		bool ProcessingOnNodes(HtmlNodeCollection nodes, List<RemarkMessage> remarkMessages)
		{
			bool ret = false;

			if (nodes != null)
			{
				foreach (HtmlNode node in nodes)
				{
					logger.Debug("テキスト以外 Name:" + node.Name + " Text:" + node.InnerText + " InnerHtml:" + node.InnerHtml);

					// 各タグに応じた処理
					switch (node.Name)
					{
						case "html":
						case "body":
						case "table":
							// 子ノード(tr) を解析する
							ret = ProcessingOnNodes(node.ChildNodes, remarkMessages);
							break;
						case "tr":
							// 子ノード(th or td) を解析する
							ret = ProcessingOnTrNodes(node.ChildNodes, remarkMessages);
							break;
						default:
							break;
					}
					Thread.Sleep(1);
				}
			}

			return (ret);
		}

		/// <summary>
		/// tr タグに対する処理
		/// </summary>
		/// <param name="nodes"></param>
		/// <param name="colCount"></param>
		/// <param name="remarkMessages"></param>
		/// <returns></returns>
		bool ProcessingOnTrNodes(HtmlNodeCollection nodes, List<RemarkMessage> remarkMessages)
		{
			bool ret = false;

			if (nodes != null)
			{
				var message = new RemarkMessage();
				int colCount = 0;

				foreach (HtmlNode node in nodes)
				{
					if ((node.Name == "th") || (node.Name == "td"))
					{
						message[(PGReliefColumn)colCount] = node.InnerText;
						colCount++;
					}
				}

				remarkMessages.Add(message);
			}

			return (ret);
		}

		/// <summary>
		/// 指摘メッセージを html で出力する
		/// </summary>
		/// <param name="remarkMessages"></param>
		public void OutputHtml(List<RemarkMessage> remarkMessages, string outputFile)
		{
			using (var file = new StreamWriter(outputFile))
			{
				bool thOutput = false;
				string text = string.Empty;

				// ヘッダー
				text += "<HTML><BODY>";
				text += "<TABLE BORDER=1 CELLSPACING=2 CELLPADDING=3>\n";

				foreach (var info in remarkMessages)
				{
					text += "<TR>";

					foreach (PGReliefColumn col in Enum.GetValues(typeof(PGReliefColumn)))
					{
						if (thOutput == false)
						{
							// 初回はタイトル行
							text += $"<TH NOWRAP>{ info[col]}</TH>";						
						}
						else
						{
							switch (col)
							{
								case PGReliefColumn.IsChecked:
									text += "<TD NOWRAP ALIGN=center>";
									break;
								case PGReliefColumn.FileName:
								case PGReliefColumn.ID:
								case PGReliefColumn.Message:
								case PGReliefColumn.Project:
									text += "<TD NOWRAP ALIGN=left>";
									break;
								case PGReliefColumn.LineNumber:
								case PGReliefColumn.Group:
								case PGReliefColumn.Number:
									text += "<TD NOWRAP ALIGN=right>";
									break;
								default:
									text += "<TD NOWRAP>";
									break;
							}
							text += $"{info[col]}</TD>";
						}
					}
					text += "</TR>\n";

					thOutput = true;
				}

				// フッター
				text += "</TABLE>\n";
				text += "</BODY></HTML>\n";

				file.Write(text);
			}
		}

		/// <summary>
		/// テキストの文字コードを変換する
		/// </summary>
		/// <param name="src">文字コードを変換したいテキスト</param>
		/// <param name="srcEnc">変換前の文字コード</param>
		/// <param name="destEnc">変換後の文字コード</param>
		/// <returns></returns>
		public static string ConvertEncoding(string src, Encoding srcEnc, Encoding destEnc)
		{
			// テキストをバイトシーケンスに変換
			byte[] srcTemp = Encoding.ASCII.GetBytes(src);
			// 文字コードの変換
			byte[] destTemp = Encoding.Convert(Encoding.ASCII, destEnc, srcTemp);
			//byte[] destTemp = Encoding.Convert(srcEnc, destEnc, srcTemp);
			// 変換後のバイトシーケンスをテキストで取得して返す
			return destEnc.GetString(destTemp);
		}
	}
}
