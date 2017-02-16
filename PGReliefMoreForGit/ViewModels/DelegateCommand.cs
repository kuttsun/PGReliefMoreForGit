using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;

namespace PGReliefMoreForGit.ViewModels
{
	/// <summary>
	/// コマンドの実装
	/// </summary>
	class DelegateCommand : ICommand
	{
		/// <summary>
		/// Execute の実体を保持します。
		/// </summary>
		private Action<object> execute;

		/// <summary>
		/// CanExecute の実態を保持します。
		/// </summary>
		private Func<object, bool> canExecute;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="execute">Execute の実体を指定</param>
		/// <param name="canExecute">CanExecute の実体を指定</param>
		public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
		{
			this.execute = execute;
			this.canExecute = canExecute;
		}

		/// <summary>
		/// コマンドを実行するかどうかに影響するような変更があった場合に発生する
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// CanExecuteChangedイベントを発行する
		/// </summary>
		public void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 現在の状態でコマンドが実行可能かどうかを決定するメソッドを定義
		/// </summary>
		/// <param name="parameter">コマンドで使用されたデータ。 コマンドにデータを渡す必要がない場合は、このオブジェクトを null に設定できる。</param>
		/// <returns>コマンドを実行できる場合は true。それ以外の場合は false。</returns>
		public bool CanExecute(object parameter)
		{
			return (canExecute == null ? true : canExecute(parameter));
		}

		/// <summary>
		/// コマンドが起動される際に呼び出すメソッドを定義
		/// </summary>
		/// <param name="parameter">コマンドで使用されたデータ。 コマンドにデータを渡す必要がない場合は、このオブジェクトを null に設定できる。</param>
		public void Execute(object parameter)
		{
			execute?.Invoke(parameter);
		}
	}
}
