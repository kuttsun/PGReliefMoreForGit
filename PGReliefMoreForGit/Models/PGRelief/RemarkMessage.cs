using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGReliefMoreForGit.Models.PGRelief
{
	/// <summary>
	/// PGRelief の１指摘事項の情報
	/// </summary>
	class RemarkMessage
	{
		/// <summary>
		/// チェック済み
		/// </summary>
		public string IsChecked { get; set; } = string.Empty;
		/// <summary>
		/// ファイル名
		/// </summary>
		public string FileName { get; set; } = string.Empty;
		/// <summary>
		/// 行番号
		/// </summary>
		public string LineNumber { get; set; } = string.Empty;
		/// <summary>
		/// グループ
		/// </summary>
		public string Group { get; set; } = string.Empty;
		/// <summary>
		/// 指摘ID
		/// </summary>
		public string ID { get; set; } = string.Empty;
		/// <summary>
		/// 指摘メッセージ
		/// </summary>
		public string Message { get; set; } = string.Empty;
		/// <summary>
		/// プロジェクト
		/// </summary>
		public string Project { get; set; } = string.Empty;
		/// <summary>
		/// 件数
		/// </summary>
		public string Number { get; set; } = string.Empty;

		/// <summary>
		/// インデクサー
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string this[PGReliefColumn type]
		{
			set
			{
				switch (type)
				{
					case PGReliefColumn.IsChecked:
						IsChecked = value;
						break;
					case PGReliefColumn.FileName:
						FileName = value;
						break;
					case PGReliefColumn.LineNumber:
						LineNumber = value;
						break;
					case PGReliefColumn.Group:
						Group = value;
						break;
					case PGReliefColumn.ID:
						ID = value;
						break;
					case PGReliefColumn.Message:
						Message = value;
						break;
					case PGReliefColumn.Project:
						Project = value;
						break;
					case PGReliefColumn.Number:
						Number = value;
						break;
					default:
						break;
				}
			}
			get
			{
				switch (type)
				{
					case PGReliefColumn.IsChecked:
						return IsChecked;
					case PGReliefColumn.FileName:
						return FileName;
					case PGReliefColumn.LineNumber:
						return LineNumber;
					case PGReliefColumn.Group:
						return Group;
					case PGReliefColumn.ID:
						return ID;
					case PGReliefColumn.Message:
						return Message;
					case PGReliefColumn.Project:
						return Project;
					case PGReliefColumn.Number:
						return Number;
					default:
						break;
				}

				return string.Empty;
			}
		}
	}
}
