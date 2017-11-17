using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Diagnostics;

using NLog;

using PGReliefMoreForGit.Models.Update;

namespace PGReliefMoreForGit
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		[STAThread()]
		public static void Main()
		{
			// タイトルを設定
			var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			var assm = Assembly.GetExecutingAssembly();
			var name = assm.GetName();
			logger.Info($"----- Start Application ----- {name.Name} [{fvi.ProductVersion} ({name.Version})] -----");

			// アップデート後の起動の場合は後始末を行う
			Update.Instance.CleanUp();

			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}

