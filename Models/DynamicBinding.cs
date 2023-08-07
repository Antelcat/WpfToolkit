using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Antelcat.Wpf.Models;

[Localizability(LocalizationCategory.Ignore)]
[DictionaryKeyProperty(nameof(TargetType))]
[ContentProperty(nameof(Bindings))]
public class DynamicContext : DependencyObject, IAddChild {
    public Type TargetType {
        get {
            VerifyAccess();
            return targetType;
        }

        set {
            VerifyAccess();
            
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            
            if (value == targetType) {
                return;
            }
            
            targetType = value;
            nameScope.Clear();
            foreach (var propertyInfo in targetType.GetProperties()) {
                if (propertyInfo.CanWrite) {
                    nameScope.RegisterName(propertyInfo.Name, propertyInfo);
                }
            }
        }
    }
    
    private Type targetType = typeof(object);
    
    /// <summary>
    ///     The collection of property setters for the target type
    /// </summary>
    [DependsOn(nameof(TargetType))]
    public Collection<DynamicBinding> Bindings {
        get {
            VerifyAccess();
            return bindings ??= new Collection<DynamicBinding>();
        }
    }

    private Collection<DynamicBinding>? bindings;
    
    private readonly NameScope nameScope = new();

    public DynamicContext() {
        NameScope.SetNameScope(this, nameScope);
    }
    
    public void AddChild(object value) {
        if (value is not DynamicBinding binding) {
            throw new ArgumentException("value must be a DynamicBinding");
        }
        
        Bindings.Add(binding);
    }
    
    public void AddText(string text) {
        throw new NotSupportedException();
    }
}

public class DynamicBinding : DependencyObject {
    [Ambient] public string? Property { get; set; }

    [DependsOn(nameof(Property))] public object? Value { get; set; }

    [DependsOn(nameof(Property))] public BindingBase? Binding { get; set; }
}