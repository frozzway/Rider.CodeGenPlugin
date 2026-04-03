using System.Collections.Generic;
using JetBrains.IDE.UI;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.UI.Components;

public class SearchableClassPicker : SearchableTypeElementPicker<IClass>
{
    public SearchableClassPicker(Lifetime lifetime, IDialogHost dialogHost, IEnumerable<IClass> classes)
        : base(lifetime, dialogHost, classes) {}

    public SearchableClassPicker(Lifetime lifetime, IDialogHost dialogHost)
        : base(lifetime, dialogHost) {}
}
