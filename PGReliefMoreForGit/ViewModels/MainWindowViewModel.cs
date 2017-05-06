﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Windows;

using NLog;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using PGReliefMoreForGit.Models;
using PGReliefMoreForGit.Models.Setting;
using PGReliefMoreForGit.Models.Update;

namespace PGReliefMoreForGit.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		/* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

		/* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

		/* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

		/* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

		Logger logger = LogManager.GetCurrentClassLogger();

		public void Initialize()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MainWindowViewModel()
		{
			// タイトルを設定
			var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			var assm = Assembly.GetExecutingAssembly();
			var name = assm.GetName();
			Title = $"{name.Name}   {fvi.ProductVersion}";

			// 設定の読み込み
			Repository = FileSetting.Instance.Repository;
			ShaHash = FileSetting.Instance.ShaHash;
			InputFile = FileSetting.Instance.InputFile;
			OutputFile = FileSetting.Instance.OutputFile;
		}

		Analysis Analysis { get; set; } = new Analysis();

		public string Title { get; set; } = string.Empty;

		#region Runボタンの処理
		private ViewModelCommand _RunCommand;
		bool _CanRun = true;

		public ViewModelCommand RunCommand
		{
			get
			{
				if (_RunCommand == null)
				{
					_RunCommand = new ViewModelCommand(Run, CanRun);
				}
				return _RunCommand;
			}
		}

		public bool CanRun()
		{
			return _CanRun;
		}

		public void Run()
		{
			// ボタンの実行可否が変化したことを通知
			_CanRun = false;
			RunCommand.RaiseCanExecuteChanged();

			try
			{
				Analysis.Run(Repository, ShaHash, InputFile, OutputFile);
				Messenger.RaiseAsync(new InformationMessage("完了しました", "Done", MessageBoxImage.Information, "RunResult"));
			}
			catch (Exception e)
			{
				Messenger.RaiseAsync(new InformationMessage($"失敗しました\n----------\n{e.Message}", "Error", MessageBoxImage.Error, "RunResult"));
			}

			// ボタンの実行可否が変化したことを通知
			_CanRun = true;
			RunCommand.RaiseCanExecuteChanged();
		}
		#endregion

		#region 更新をチェックする
		/*
		private ViewModelCommand _CheckUpdateCommand;
		bool _CanCheckUpdate = true;

		public ViewModelCommand CheckUpdateCommand
		{
			get
			{
				if (_CheckUpdateCommand == null)
				{
					_CheckUpdateCommand = new ViewModelCommand(CheckUpdate, CanCheckUpdate);
				}
				return _CheckUpdateCommand;
			}
		}

		public bool CanCheckUpdate()
		{
			return _CanCheckUpdate;
		}
		*/

		Update update = Update.Instance;
		/// <summary>
		/// アップデートが存在するかどうかチェックする
		/// </summary>
		/// <returns></returns>
		public bool? CheckUpdate(out string latestVersion)
		{
			return update.CheckUpdate(out latestVersion);
		}

		/// <summary>
		/// アップデートを実行する
		/// </summary>
		/// <returns></returns>
		public bool RunUpdate()
		{
			return update.RunUpdate();
		}
		#endregion

		#region エディットボックスの値
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
				//OnPropertyChanged(nameof(Repository));
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
		#endregion
	}
}
