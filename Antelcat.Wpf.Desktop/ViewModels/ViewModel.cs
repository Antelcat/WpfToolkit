using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Antelcat.Wpf.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antelcat.Wpf.Desktop.ViewModels;


public enum Language
{
    English,
    Chinese,
    Japanese
}

public partial class ViewModel : ObservableObject
{
    public Language Language
    {
        get => language; 
        set
        {
            LangExtension.Culture = value switch
            {
                Language.Chinese => new CultureInfo("zh"),
                Language.English => new CultureInfo("en"),
                Language.Japanese => new CultureInfo("jp"),
                _ => new CultureInfo(0)
            };
            language = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Description));
        }
    }

    private Language language;

    public Language Description => Language; 
    
    public static Language[] Languages { get; } = { Language.English, Language.Chinese, Language.Japanese };

}