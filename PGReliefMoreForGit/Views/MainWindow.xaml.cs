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
		private async void ButtonRun_Click(object sender, RoutedEventArgs e)
		{
			var controller = await this.ShowProgressAsync("Processing", "Please wait...");

			controller.SetIndeterminate();

			var vm = DataContext as MainWindowViewModel;

			await Task.Run(() => vm.Run());

			await controller.CloseAsync();
		}
	}
}
