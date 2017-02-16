using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using PGReliefMoreForGit.Models;
using PGReliefMoreForGit.Models.Setting;

namespace PGReliefMoreForGit.ViewModels
{
	class MainWindowsVM : ViewModelBase
	{
		public string Title { get; set; } = string.Empty;
		
		/// <summary>
		/// Git のローカルリポジトリへのパス
		/// </summary>
		public string Repository
		{
			get
			{
				return repository;
			}
			set
			{
				repository = value;
				OnPropertyChanged(nameof(Repository));
			}
		}
		string repository = string.Empty;

		/// <summary>
		/// ワーキングツリーと比較するコミットの SHA ハッシュ値
		/// </summary>
		public string ShaHash { get; set; } = string.Empty;
		/// <summary>
		/// 入力ファイル（PGRelief で出力した html ファイル）
		/// </summary>
		public string InputFile { get; set; } = string.Empty;
		/// <summary>
		/// 出力ファイル（html ファイル）
		/// </summary>
		public string OutputFile { get; set; } = string.Empty;

		/// <summary>
		/// 実行ボタン
		/// </summary>
		public DelegateCommand Button_Run { get; set; }

		Analysis Analysis { get; set; } = new Analysis();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MainWindowsVM()
		{
			// タイトルを設定
			var assm = Assembly.GetExecutingAssembly();
			var name = assm.GetName();
			Title = $"{name.Name} {name.Version}";

			// 設定の読み込み
			Repository = FileSetting.Instance.Repository;
			ShaHash = FileSetting.Instance.ShaHash;
			InputFile = FileSetting.Instance.InputFile;
			OutputFile = FileSetting.Instance.OutputFile;

			// 実行ボタン
			Button_Run = new DelegateCommand(
						x => Analysis.Run(Repository, ShaHash, InputFile, OutputFile),
						x => true
			);
		}
	}
}
