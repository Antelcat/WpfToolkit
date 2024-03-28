using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Antelcat.Wpf.Attachments;

/// <summary>
/// Visibility更改时播放动画
/// </summary>
/// https://www.codeproject.com/Articles/57175/WPF-How-To-Animate-Visibility-Property
public class VisibilityAnimationAttach {
	private static readonly Dictionary<FrameworkElement, bool> AttachedElements = new();

	static VisibilityAnimationAttach() {
		UIElement.VisibilityProperty.AddOwner(
				typeof(FrameworkElement),
				new FrameworkPropertyMetadata(
					Visibility.Visible,
					VisibilityChanged,
					CoerceVisibility));
	}

	public static readonly DependencyProperty AnimationTypeProperty = DependencyProperty.RegisterAttached(
		"AnimationType", typeof(VisibilityAnimationType), typeof(VisibilityAnimationAttach), 
		new PropertyMetadata(default(VisibilityAnimationType), VisibilityAnimationType_OnChanged));

	public static void SetAnimationType(DependencyObject element, VisibilityAnimationType value) {
		element.SetValue(AnimationTypeProperty, value);
	}

	public static VisibilityAnimationType GetAnimationType(DependencyObject element) {
		return (VisibilityAnimationType)element.GetValue(AnimationTypeProperty);
	}

	private static void VisibilityAnimationType_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is not FrameworkElement element) {
			throw new NotSupportedException($"Only {nameof(FrameworkElement)} supports {nameof(VisibilityAnimationAttach)}");
		}

		if (e.NewValue is VisibilityAnimationType.None) {
			AttachedElements.Remove(element);
		} else {
			AttachedElements.Add(element, false);
		}
	}

	public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.RegisterAttached(
		"EasingFunction", typeof(IEasingFunction), typeof(VisibilityAnimationAttach), 
		new PropertyMetadata(new CubicEase { EasingMode = EasingMode.EaseInOut }));

	public static void SetEasingFunction(DependencyObject element, IEasingFunction value) {
		element.SetValue(EasingFunctionProperty, value);
	}

	public static IEasingFunction GetEasingFunction(DependencyObject element) {
		return (IEasingFunction)element.GetValue(EasingFunctionProperty);
	}
	
	public static readonly DependencyProperty DurationProperty = DependencyProperty.RegisterAttached(
		"Duration", typeof(Duration), typeof(VisibilityAnimationAttach), 
		new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(400))));
	
	public static void SetDuration(DependencyObject element, Duration value) {
		element.SetValue(DurationProperty, value);
	}
	
	public static Duration GetDuration(DependencyObject element) {
		return (Duration)element.GetValue(DurationProperty);
	}

	/// <summary>
	/// Visibility changed
	/// </summary>
	/// <param name="dependencyObject">Dependency object</param>
	/// <param name="e">e</param>
	private static void VisibilityChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) { }

	/// <summary>
	/// Coerce visibility
	/// </summary>
	/// <param name="dependencyObject">Dependency object</param>
	/// <param name="baseValue">Base value</param>
	/// <returns>Coerced value</returns>
	private static object CoerceVisibility(DependencyObject dependencyObject, object baseValue) {
		if (dependencyObject is not FrameworkElement element) {
			return baseValue;
		}

		if (!AttachedElements.ContainsKey(element)) {
			return baseValue;
		}

		// Cast to type safe value
		var visibility = (Visibility)baseValue;

		// If Visibility value hasn't change, do nothing.
		// This can happen if the Visibility property is set using data binding 
		// and the binding source has changed but the new visibility value 
		// hasn't changed.
		if (visibility == element.Visibility) {
			return baseValue;
		}

		// Update animation flag
		// If animation already started, don't restart it (otherwise, infinite loop)
		if (UpdateAnimationStartedFlag(element)) {
			return baseValue;
		}

		// If we get here, it means we have to start fade in or fade out animation. 
		// In any case return value of this method will be Visibility.Visible, 
		// to allow the animation.
		var opacityAnimation = new DoubleAnimation {
			Duration = GetDuration(element),
			EasingFunction = GetEasingFunction(element)
		};

		// When animation completes, set the visibility value to the requested 
		// value (baseValue)
		opacityAnimation.Completed += (_, _) =>
		{
			if (visibility == Visibility.Visible) {
				// In case we change into Visibility.Visible, the correct value 
				// is already set, so just update the animation started flag
				UpdateAnimationStartedFlag(element);
			} else {
				// This will trigger value coercion again 
				// but UpdateAnimationStartedFlag() function will return true 
				// this time, thus animation will not be triggered. 
				if (BindingOperations.IsDataBound(element, UIElement.VisibilityProperty)) {
					// Set visibility using bounded value
					var bindingValue = BindingOperations.GetBinding(element, UIElement.VisibilityProperty);
					BindingOperations.SetBinding(element, UIElement.VisibilityProperty, bindingValue!);
				} else {
					// No binding, just assign the value
					element.Visibility = visibility;
				}
			}
		};

		if (visibility is Visibility.Collapsed or Visibility.Hidden) {
			// Fade out by animating opacity
			opacityAnimation.From = 1.0;
			opacityAnimation.To = 0.0;
		} else {
			// Fade in by animating opacity
			opacityAnimation.From = 0.0;
			opacityAnimation.To = 1.0;
		}

		// Start animation
		element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);

		// Make sure the element remains visible during the animation
		// The original requested value will be set in the completed event of 
		// the animation
		return Visibility.Visible;
	}

	/// <summary>
	/// Update animation started flag or a given framework element
	/// </summary>
	/// <param name="frameworkElement">Given framework element</param>
	/// <returns>Old value of animation started flag</returns>
	private static bool UpdateAnimationStartedFlag(FrameworkElement frameworkElement) {
		var animationStarted = AttachedElements[frameworkElement];
		AttachedElements[frameworkElement] = !animationStarted;

		return animationStarted;
	}
}

public enum VisibilityAnimationType {
	/// <summary>
	/// 无动画
	/// </summary>
	None,

	/// <summary>
	/// 渐变
	/// </summary>
	Fade,
	
	/// <summary>
	/// 缩放
	/// </summary>
	Zoom,
}