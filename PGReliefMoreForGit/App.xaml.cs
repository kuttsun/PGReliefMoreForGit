using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using NLog;

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
			var assm = Assembly.GetExecutingAssembly();
			var name = assm.GetName();
			logger.Info($"----- Start Application ----- {name.Name} {name.Version} -----");

			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}
