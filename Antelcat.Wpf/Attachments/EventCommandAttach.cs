using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Antelcat.Wpf.Attachments;

public class EventCommandAttach
{
    private readonly static Dictionary<UIElement, RoutedEventHandler> AttachedElements = new();

    public static readonly DependencyProperty EventNameProperty = DependencyProperty.RegisterAttached(
        "EventName", typeof(string), typeof(EventCommandAttach),
        new PropertyMetadata(default(string), EventName_OnChanged));

    private static void EventName_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            throw new NotSupportedException($"Only {nameof(UIElement)} supports {nameof(EventCommandAttach)}");
        }
        var type = d.GetType();
        var eventName = (string)e.NewValue;
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
        FieldInfo? fieldInfo = null;
        while (type != null)
        {
            if ((fieldInfo = type.GetField(eventName, bindingFlags)) != null ||
                (fieldInfo = type.GetField(eventName + "Event", bindingFlags)) != null)
            {
                break;
            }
            type = type.BaseType;
        }
        if (fieldInfo == null)
        {
            throw new NotSupportedException($"Event {eventName} not found in {d.GetType()}");
        }
        if (fieldInfo.GetValue(d) is not RoutedEvent routedEvent)
        {
            throw new NotSupportedException($"Field {eventName} is not a {nameof(RoutedEvent)}");
        }
        if (e.OldValue != null)
        {
            if (AttachedElements.TryGetValue(element, out var handler))
            {
                element.RemoveHandler(routedEvent, handler);
                AttachedElements.Remove(element);
            }
        }
        if (e.NewValue != null)
        {
            var command = GetCommand(element);
            if (command == null) return;
            var handler = new RoutedEventHandler((_, args) =>
            {
                if (command.CanExecute(args))
                {
                    command.Execute(args);
                }
            });
            element.AddHandler(routedEvent, handler);
            AttachedElements.Add(element, handler);
        }
    }

    public static void SetEventName(DependencyObject element, string? value)
    {
        element.SetValue(EventNameProperty, value);
    }

    public static string? GetEventName(DependencyObject element)
    {
        return (string?)element.GetValue(EventNameProperty);
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command", typeof(ICommand), typeof(EventCommandAttach),
        new PropertyMetadata(default(ICommand), Command_OnChanged));

    private static void Command_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var eventName = GetEventName(d);
        if (eventName != null)
        {
            EventName_OnChanged(d, new DependencyPropertyChangedEventArgs(EventNameProperty, eventName, eventName));
        }
    }

    public static void SetCommand(DependencyObject element, ICommand? value)
    {
        element.SetValue(CommandProperty, value);
    }

    public static ICommand? GetCommand(DependencyObject element)
    {
        return (ICommand?)element.GetValue(CommandProperty);
    }
}