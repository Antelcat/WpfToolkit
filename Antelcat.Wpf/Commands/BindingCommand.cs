using System;
using System.Windows;
using System.Windows.Input;

namespace Antelcat.Wpf.Commands;

public class BindingCommand : DependencyObject, ICommand {
	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
		nameof(Command), typeof(ICommand), typeof(BindingCommand), 
		new PropertyMetadata(default(ICommand?), CommandProperty_OnChanged));

	public ICommand? Command {
		get => (ICommand?)GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	private static void CommandProperty_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		var command = (BindingCommand)d;

		if (e.OldValue is ICommand oldCommand) {
			oldCommand.CanExecuteChanged -= command.CanExecuteChanged;
		}

		if (e.NewValue is ICommand newCommand) {
			newCommand.CanExecuteChanged += command.CanExecuteChanged;
		}

		command.CanExecuteChanged?.Invoke(command, EventArgs.Empty);
	}

	public bool CanExecute(object? parameter) {
		return Command?.CanExecute(parameter) ?? false;
	}

	public void Execute(object? parameter) {
		Command?.Execute(parameter);
	}

	public event EventHandler? CanExecuteChanged;
}