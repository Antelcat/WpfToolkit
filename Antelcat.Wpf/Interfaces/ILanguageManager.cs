using System.ComponentModel;
using System.Globalization;

namespace Antelcat.Wpf.Interfaces;

public interface ILanguageManager : INotifyPropertyChanged {
    CultureInfo CurrentCulture { get; set; }
}