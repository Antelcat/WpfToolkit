using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;

namespace Antelcat.Wpf.Utils;

public class EmptyWindowAutomationPeer(FrameworkElement owner) : FrameworkElementAutomationPeer(owner)
{
    protected override string GetNameCore() => nameof(EmptyWindowAutomationPeer);

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Window;

    protected override List<AutomationPeer> GetChildrenCore() => [];
}