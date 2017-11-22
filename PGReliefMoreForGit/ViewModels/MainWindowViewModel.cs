using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Windows;

using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

using NLog;

using PGReliefMoreForGit.Models;
using PGReliefMoreForGit.Models.Settings;
using PGReliefMoreForGit.Models.Update;

namespace PGReliefMoreForGit.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        FileSetting fileSetting = FileSetting.Instance;

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
            Repository = fileSetting.Repository;
            ShaHash = fileSetting.ShaHash;
            InputFile = fileSetting.InputFile;
            OutputFile = fileSetting.OutputFile;
        }

        Analysis Analysis { get; set; } = new Analysis();

        public string Title { get; set; } = string.Empty;

        #region Runボタンの処理
        /// <summary>
        /// フィルタリングを実行する
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool Run(out string reason)
        {
            reason = string.Empty;

            try
            {
                Analysis.Run(Repository, ShaHash, InputFile, OutputFile);
                return true;
            }
            catch (Exception e)
            {
                reason = e.Message;
                logger.Error(e.Message);
            }

            return false;
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

        #region 入力値
        /// <summary>
        /// Git リポジトリへのパス
        /// </summary>
        public string Repository
        {
            get { return repository; }
            set
            {
                SetProperty(ref repository, value);
                fileSetting.Repository = value;
            }
        }
        string repository = string.Empty;
        /// <summary>
        /// 比較対象のコミットの SHA ハッシュ値
        /// </summary>
        public string ShaHash
        {
            get { return shaHash; }
            set
            {
                SetProperty(ref shaHash, value);
                fileSetting.ShaHash = value;
            }
        }
        string shaHash = string.Empty;
        /// <summary>
        /// 入力ファイル（PGRelief で出力した html ファイル）
        /// </summary>
        public string InputFile
        {
            get { return inputFile; }
            set
            {
                SetProperty(ref inputFile, value);
                fileSetting.InputFile = value;
            }
        }
        string inputFile = string.Empty;
        /// <summary>
        /// 出力ファイル
        /// </summary>
        public string OutputFile
        {
            get { return outputFile; }
            set
            {
                SetProperty(ref outputFile, value);
                fileSetting.OutputFile = value;
            }
        }
        string outputFile = string.Empty;
        #endregion

        #region ファイルの入出力
        public bool Load(string fileName)
        {
            try
            {
                fileSetting.Load(fileName);

                // メンバーの更新と View の更新
                // これもっと良いやり方ないだろうか？
                fileSetting = FileSetting.Instance;
                Repository = fileSetting.Repository;
                ShaHash = fileSetting.ShaHash;
                InputFile = fileSetting.InputFile;
                OutputFile = fileSetting.OutputFile;

                return true;
            }
            catch (Exception e)
            {
                logger.Error(e);
                return false;
            }
        }

        public bool Save(string fileName)
        {
            try
            {
                fileSetting.Save(fileName);
                return true;
            }
            catch(Exception e)
            {
                logger.Error(e);
                return false;
            }
        }
        #endregion
    }
}
