using System;
using System.Windows.Input;

namespace Antelcat.Wpf.Commands; 

public class SimpleCommand : ICommand {
	private readonly Action<object?>? action;

	public SimpleCommand() { }

	public SimpleCommand(Action action) {
		this.action = _ => action();
	}

	public SimpleCommand(Action<object?> action) {
		this.action = action;
	}

	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter) {
		action?.Invoke(parameter);
	}

	public event EventHandler? CanExecuteChanged;
}