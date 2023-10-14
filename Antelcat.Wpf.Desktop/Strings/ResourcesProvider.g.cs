using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Antelcat.Wpf.Interfaces;
#nullable enable

namespace Antelcat.Wpf.Desktop.Strings;

public partial class ResourcesProvider  : ILanguageManager
{
    public static string X_Key = string.Empty;

    public static ResourcesProvider Instance 
    { 
        get => instance ??= Application.Current.TryFindResource(X_Key) as ResourcesProvider ?? new ResourcesProvider(); 
        private set => instance = value; 
    }
    private static ResourcesProvider? instance;

    public ResourcesProvider() => Instance = this; 

    private static string? CultureInfoStr;

    public CultureInfo CurrentCulture { get => Culture!; set => Culture = value; }

    public static CultureInfo? Culture
    {
        get => Resources.Culture;
        set
        {
            if (value == null) return;
            if (Equals(CultureInfoStr, value.EnglishName)) return;
            Resources.Culture = value;
            CultureInfoStr = value.EnglishName;
            Instance.UpdateResourcess();
        }
    }

    public static string? GetResources(string key) => Resources.ResourceManager.GetString(key, Culture);

    public static void SetResources(DependencyObject dependencyObject, DependencyProperty dependencyProperty, string key)
    {
        BindingOperations.SetBinding(dependencyObject, dependencyProperty, new Binding(key)
        {
            Source = Instance,
            Mode = BindingMode.OneWay
        });
    }

	private void UpdateResourcess()
    {
		OnPropertyChanged(nameof(Language));
		OnPropertyChanged(nameof(English));
		OnPropertyChanged(nameof(Chinese));
		OnPropertyChanged(nameof(Description));
    }

    /// <summary>
    /// Language
    /// </summary>
	public string Language => Resources.Language;

    /// <summary>
    /// English
    /// </summary>
	public string English => Resources.English;

    /// <summary>
    /// Chinese
    /// </summary>
	public string Chinese => Resources.Chinese;

    /// <summary>
    /// Description
    /// </summary>
	public string Description => Resources.Description;


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public partial class ResourcesKeys
{
    /// <summary>
    /// Language
    /// </summary>
	public static string Language = nameof(Language);

    /// <summary>
    /// English
    /// </summary>
	public static string English = nameof(English);

    /// <summary>
    /// Chinese
    /// </summary>
	public static string Chinese = nameof(Chinese);

    /// <summary>
    /// Description
    /// </summary>
	public static string Description = nameof(Description);

}
