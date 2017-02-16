using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGReliefMoreForGit.Models.Git
{
	/// <summary>
	/// 変更領域
	/// </summary>
	class ChangedLine
	{ 
		/// <summary>
		/// 変更後のファイルパス 
		/// </summary>
		public string NewPath { get; set; } = string.Empty;
		/// <summary>
		/// 変更開始行
		/// </summary>
		public int StartLine { get; set; } = 0;
		/// <summary>
		/// 変更行数
		/// </summary>
		public int NumberOfChangedLines { get; set; } = 0;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ChangedLine(string newPath, int startLine, int numberOfChangedLines)
		{
			NewPath = newPath;
			StartLine = startLine;
			NumberOfChangedLines = numberOfChangedLines;
		}
	}
}
