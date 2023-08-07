using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Antelcat.Wpf.Models;

namespace Antelcat.Wpf.Controls;

/// <summary>
/// 提供可以依赖注入的动态控件
/// </summary>
public class DynamicControl : ContentControl, INameScope {
    public static readonly DependencyProperty DynamicContextProperty = DependencyProperty.Register(
        nameof(DynamicContext), typeof(DynamicContext), typeof(DynamicControl), 
        new PropertyMetadata(default(DynamicContext?)));

    public DynamicContext? DynamicContext {
        get => (DynamicContext?)GetValue(DynamicContextProperty);
        set => SetValue(DynamicContextProperty, value);
    }

    protected override void OnContentChanged(object oldContent, object newContent) {
        if (DynamicContext == null || newContent is not FrameworkElement element) {
            return;
        }

        var newType = newContent.GetType();
        foreach (var binding in DynamicContext.Bindings) {
            var propertyName = binding.Property ??
                               throw new NullReferenceException(nameof(binding.Property));
            var property = newType.GetField(
                propertyName + "Property",
                BindingFlags.Static | BindingFlags.Public);
            if (property == null) continue;
            var dependencyProperty = property.GetValue(null) as DependencyProperty ??
                                     throw new NullReferenceException(property.Name);
            if (binding.Binding != null) {
                element.SetBinding(
                    dependencyProperty,
                    binding.Binding);
            } else {
                element.SetValue(dependencyProperty, binding.Value);
            }
        }

        base.OnContentChanged(oldContent, newContent);
    }
}