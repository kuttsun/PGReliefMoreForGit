using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using PGReliefMoreForGit.ViewModels;

namespace PGReliefMoreForGit.Views
{
	/* 
	 * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		// とりあえずコードビハインドに書くが、出来れば Xaml だけで完結させたい

		// 実行
		private async void ButtonRun_Click(object sender, RoutedEventArgs e)
		{
			var controller = await this.ShowProgressAsync("Processing", "Please wait...");

			controller.SetIndeterminate();

			var vm = DataContext as MainWindowViewModel;

			await Task.Run(() => vm.Run());

			await controller.CloseAsync();
		}

		// GitHub のページへアクセス
		private void MenuGitHubPage_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://github.com/kuttsun/PGReliefMoreForGit");
		}

		// 更新のチェック
		private async void MenuCheckUpdate_Click(object sender, RoutedEventArgs e)
		{
			// プログレスダイアログを表示
			var controller1 = await this.ShowProgressAsync("Checking Update", "Please wait...");

			// プログレスバー不確定モードにする
			controller1.SetIndeterminate();

			var vm = DataContext as MainWindowViewModel;

			string latestVersion = string.Empty;
			bool? checkResult = await Task.Run(() => vm.CheckUpdate(out latestVersion));

			// プログレスダイアログを閉じる
			await controller1.CloseAsync();

			// 結果に応じてダイアログを表示
			switch (checkResult)
			{
				case true:
					var metroDialogSettings = new MetroDialogSettings()
					{
						AffirmativeButtonText = "Yes",
						NegativeButtonText = "No",
					};
					var select = await this.ShowMessageAsync("Update Available", $"A new version ({latestVersion}) is available.\nDo you update now ?", MessageDialogStyle.AffirmativeAndNegative, metroDialogSettings);
					if (select == MessageDialogResult.Affirmative)
					{
						// プログレスダイアログを表示
						var controller2 = await this.ShowProgressAsync("Run Update", "Please wait...");

						// プログレスバー不確定モードにする
						controller2.SetIndeterminate();

						bool updateResult = await Task.Run(() => vm.RunUpdate());

						// プログレスダイアログを閉じる
						await controller2.CloseAsync();

						if (updateResult == true)
						{
							// 成功したら再起動するため、アプリケーションを終了する
							Close();
						}
					}
					break;
				case false:
					await this.ShowMessageAsync("No Updates", "You already have the latest version.");
					break;
				default:
					await this.ShowMessageAsync("Network Error", "Check your network connection and try again.");
					break;
			}
		}

		// アプリ終了
		private void MenuExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
