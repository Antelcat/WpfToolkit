using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using Antelcat.Wpf.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antelcat.Wpf.Desktop.ViewModels;

public partial class ViewModel : ObservableObject
{
    public CultureInfo? Language
    {
        get => language; 
        set
        {
            if (value is null) return;
            language              = value;
            I18NExtension.Culture = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Description));
        }
    }

    private CultureInfo? language;

    public CultureInfo? Description => Language;

    public static CultureInfo[] Languages { get; } =
    {
        new("zh"),
        new("en"),
        new("jp"),
    };

}