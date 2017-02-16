using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Serialization;

namespace PGReliefMoreForGit.Models.Setting
{
	/// <summary>
	/// ユーザーが設定ファイルとして個別に保存する設定（Singleton）
	/// </summary>
	public sealed class FileSetting
	{
		/// <summary>
		/// 名前
		/// </summary>
		public string Repository { get; set; } = string.Empty;
		/// <summary>
		/// 苗字
		/// </summary>
		public string ShaHash { get; set; } = string.Empty;
		/// <summary>
		/// 入力ファイル（PGRelief で出力した html ファイル）
		/// </summary>
		public string InputFile { get; set; } = string.Empty;
		/// <summary>
		/// 出力ファイル
		/// </summary>
		public string OutputFile { get; set; } = string.Empty;

		/// <summary>
		/// 自分のインスタンス
		/// </summary>
		public static FileSetting Instance
		{
			get
			{
				if (instance == null)
				{
					// XMLファイルを読み込んでデシリアライズする
					try
					{
						XmlSerializer serializer = new XmlSerializer(typeof(FileSetting));
						using (FileStream fs = new FileStream("FileSetting.xml", FileMode.Open, FileAccess.Read))
						{
							instance = (FileSetting)serializer.Deserialize(fs);
						}
					}
					catch
					{
						instance = new FileSetting();
					}
				}
				return instance;
			}
		}
		static FileSetting instance = null;

		/// <summary>
		/// コンストラクタ（Singleton）
		/// </summary>
		private FileSetting() { }
	}
}
