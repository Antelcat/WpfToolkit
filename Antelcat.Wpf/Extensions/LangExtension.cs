using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents.Serialization;
using System.Windows.Markup;
using Antelcat.Wpf.Interfaces;

namespace Antelcat.Wpf.Extensions;

public static class LangCacheExtension
{
	/// <summary>
	/// 设置为当前语言
	/// </summary>
	/// <param name="culture"></param>
	public static void SetCulture(this CultureInfo culture) => LangExtension.Culture = culture;

	public static void Register(this ILanguageManager instance) => LangExtension.RegisterLanguageSource(instance);
}

public class LangExtension : MarkupExtension
{
	private static readonly ExpandoObject Target = new();

	private static readonly List<ILanguageManager> Providers = new();

	private static readonly List<Action> InitActions = new();

	private static bool generated;

	public static CultureInfo Culture
	{
		set
		{
			Providers.ForEach(x => x.CurrentCulture = value);
			culture = value;
		}
	}

	private static CultureInfo? culture;

	public static void RegisterLanguageSource(Func<ILanguageManager> provider)
	{
		if (generated) RegisterLanguageSource(provider());
		else InitActions.Insert(0, () => RegisterLanguageSource(provider()));
	}

	public static void RegisterLanguageSource(ILanguageManager instance)
	{
		if (Providers.Contains(instance)) return;
		var props  = instance.GetType().GetProperties();
		var target = (IDictionary<string, object>)Target!;

		void Update(object o, string s)
		{
			var val = Array.Find(props, x => x.Name.Equals(s))?.GetValue(o);
			if (val != null)
			{
				target[s] = val;
			}
		}

		instance.PropertyChanged += (o, e) => Update(o!, e.PropertyName!);
		foreach (var prop in props)
		{
			Update(instance, prop.Name);
		}

		if (culture != null)
		{
			instance.CurrentCulture = culture;
		}

		Providers.Add(instance);
	}

	private readonly DependencyObject proxy;

	public LangExtension()
	{
		proxy = new DependencyObject();
		if (generated) return;
		InitActions.ForEach(x => x());
		InitActions.Clear();
		generated = true;
	}


	public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
		nameof(Key),
		typeof(object),
		typeof(LangExtension),
		new PropertyMetadata(default));

	public object? Key
	{
		get => proxy.GetValue(KeyProperty);
		set => proxy.SetValue(KeyProperty, value);
	}

	public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
		nameof(Source),
		typeof(Binding),
		typeof(LangExtension),
		new PropertyMetadata(default(Binding)));

	public Binding? Source
	{
		get => (Binding)proxy.GetValue(SourceProperty);
		set => proxy.SetValue(SourceProperty, value);
	}

	private static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.RegisterAttached(
		"TargetProperty",
		typeof(DependencyProperty),
		typeof(LangExtension),
		new PropertyMetadata(default(DependencyProperty)));

	private static void SetTargetProperty(DependencyObject element, DependencyProperty value)
		=> element.SetValue(TargetPropertyProperty, value);

	private static DependencyProperty GetTargetProperty(DependencyObject element)
		=> (DependencyProperty)element.GetValue(TargetPropertyProperty);
	
	public IValueConverter? Converter { get; set; }

	public object? ConverterParameter { get; set; }

	public override object? ProvideValue(IServiceProvider serviceProvider)
	{
		if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget provideValueTarget)
			return this;
		if (provideValueTarget.TargetObject.GetType().FullName == "System.Windows.SharedDp") return this;
		if (provideValueTarget.TargetObject is not DependencyObject targetObject) return this;
		if (provideValueTarget.TargetProperty is not DependencyProperty targetProperty) return this;

		switch (Key)
		{
			case string key:
			{
				var binding = CreateLangBinding(key);
				BindingOperations.SetBinding(targetObject, targetProperty, binding);
				return binding.ProvideValue(serviceProvider);
			}
			case Binding keyBinding when targetObject is FrameworkElement element:
			{
				if (element.DataContext != null)
				{
					return SetLangBinding(element,
							targetProperty,
							keyBinding.Path,
							element.DataContext)?
						.ProvideValue(serviceProvider);
				}

				SetTargetProperty(element, targetProperty);
				element.DataContextChanged += LangExtension_DataContextChanged;

				break;
			}
			case Binding keyBinding when targetObject is FrameworkContentElement element:
			{
				if (element.DataContext != null)
				{
					return SetLangBinding(element,
							targetProperty, 
							keyBinding.Path, 
							element.DataContext)?
						.ProvideValue(serviceProvider);
				}

				SetTargetProperty(element, targetProperty);
				element.DataContextChanged += LangExtension_DataContextChanged;

				break;
			}
		}

		return string.Empty;
	}

	private void LangExtension_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		switch (sender)
		{
			case FrameworkElement element:
			{
				element.DataContextChanged -= LangExtension_DataContextChanged;
				if (Key is not Binding keyBinding) return;

				var targetProperty = GetTargetProperty(element);
				SetTargetProperty(element, null!);
				SetLangBinding(element, targetProperty, keyBinding.Path, element.DataContext);
				break;
			}
			case FrameworkContentElement element:
			{
				element.DataContextChanged -= LangExtension_DataContextChanged;
				if (Key is not Binding keyBinding) return;

				var targetProperty = GetTargetProperty(element);
				SetTargetProperty(element, null!);
				SetLangBinding(element, targetProperty, keyBinding.Path, element.DataContext);
				break;
			}
		}
	}

	private BindingBase? SetLangBinding(
		DependencyObject targetObject,
		DependencyProperty? targetProperty,
		PropertyPath path,
		object dataContext)
	{
		if (targetProperty == null) return null;

		BindingOperations.SetBinding(targetObject,
			targetProperty,
			new Binding
			{
				Path   = path,
				Source = dataContext,
				Mode   = BindingMode.OneWay,
			});

		var key = targetObject.GetValue(targetProperty) as string;
		if (string.IsNullOrEmpty(key)) return null;

		var binding = CreateLangBinding(key);
		BindingOperations.SetBinding(targetObject, targetProperty, binding);
		return binding;
	}

	private BindingBase CreateLangBinding(string key)
	{
		var ret = new Binding(key)
		{
			Converter           = Converter,
			ConverterParameter  = ConverterParameter,
			Source              = TryFind(Target, key),
			Mode                = BindingMode.OneWay
		};
		if (Source == null)
		{
			ret.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
			return ret;
		}

		var multi = new MultiBinding
		{
			Converter           = new MultiValueLangConverter(Target, Converter, ConverterParameter, Key is string),
			Mode                = BindingMode.OneWay,
			UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
			ConverterParameter  = ConverterParameter
		};
		multi.Bindings.Add(ret);
		multi.Bindings.Add(Source);
		return multi;
	}

	private static object TryFind(IDictionary<string, object?> target, string key)
	{
		if (!target.ContainsKey(key))
		{
			target[key] = key;
		}

		return target;
	}

	private class MultiValueLangConverter : IMultiValueConverter
	{
		private readonly IDictionary<string,object?> resource;
		private readonly IValueConverter?            converter;
		private readonly object?                     converterParameter;
		private readonly bool                        replace;
		public MultiValueLangConverter(IDictionary<string, object?> resource,
			IValueConverter? converter,
			object? converterParameter, 
			bool replace)
		{
			this.resource           = resource;
			this.converter          = converter;
			this.converterParameter = converterParameter;
			this.replace            = replace;
		}

		public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var val = resource.TryGetValue(values[1].ToString() ?? string.Empty, out var ret)
				?  ret
				: values[1];
			if (replace)
			{
				val = string.Format(values[0] as string ?? string.Empty, val);
			}
			return converter?.Convert(val, targetType, converterParameter, culture) ?? val;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}