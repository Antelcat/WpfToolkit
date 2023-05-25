using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using System.Windows.Markup;

namespace Antelcat.Wpf.Commands;

/// <summary>
///     This is a command that simply aggregates other commands into a group.
///     This command's CanExecute logic delegates to the CanExecute logic of
///     all the child commands.  When executed, it calls the Execute method
///     on each child command sequentially.
/// </summary>
[ContentProperty("Commands")]
public class AggregateCommand : ICommand {
	#region Commands

	/// <summary>
	///     Returns the collection of child commands.  They are executed
	///     in the order that they exist in this collection.
	/// </summary>
	public ObservableCollection<ICommand> Commands {
		get {
			if (commands == null) {
				commands = new ObservableCollection<ICommand>();
				commands.CollectionChanged += Commands_OnCollectionChanged;
			}

			return commands;
		}
	}

	private ObservableCollection<ICommand>? commands;

	private void Commands_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		// We have a new child command so our ability to execute may have changed.
		OnCanExecuteChanged();

		if (e.NewItems is { Count: > 0 }) {
			foreach (ICommand cmd in e.NewItems) {
				cmd.CanExecuteChanged += ChildCommand_OnCanExecuteChanged;
			}
		}

		if (e.OldItems is { Count: > 0 }) {
			foreach (ICommand cmd in e.OldItems) {
				cmd.CanExecuteChanged -= ChildCommand_OnCanExecuteChanged;
			}
		}
	}

	private void ChildCommand_OnCanExecuteChanged(object? sender, EventArgs e) {
		// Bubble up the child commands CanExecuteChanged event so that
		// it will be observed by WPF.
		OnCanExecuteChanged();
	}

	#endregion // Commands

	#region ICommand Members

	public bool CanExecute(object? parameter) {
		return Commands.All(cmd => cmd.CanExecute(parameter));
	}

	public event EventHandler? CanExecuteChanged;

	protected virtual void OnCanExecuteChanged() {
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}

	public void Execute(object? parameter) {
		foreach (var cmd in Commands) {
			cmd.Execute(parameter);
		}
	}

	#endregion
}