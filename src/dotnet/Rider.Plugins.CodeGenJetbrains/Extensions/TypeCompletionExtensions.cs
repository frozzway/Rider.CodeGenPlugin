using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.I18n;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.Application.UI.Controls.JetPopupMenu.Detail;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.Collections.Viewable;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.Goto.ProvidersAPI;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Feature.Services.Psi;
using JetBrains.ReSharper.Feature.Services.UI.Automation;
using JetBrains.ReSharper.Feature.Services.UI.CompletionPicker;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.UI.RichText;
using JetBrains.Util;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

extern alias rt;
extern alias jb;

public static class TypeCompletionExtensions
{
    public static BeTextBox WithTypeCompletionShort(
    this BeTextBox model,
    ISolution solution,
    Lifetime lifetime,
    PsiLanguageType language,
    bool alltypes = false,
    Predicate<IDeclaredElement>? extraFilter = null)
  {
    // Проверка скопирована "как есть"
    if (!CompletionBeTextBoxExtensions.CanInitCompletion)
      return model;

    IShellLocks component = Shell.Instance.GetComponent<IShellLocks>();

    // Создание TypeChooser скопировано
    TypeChooser typeChooser = new TypeChooser(lifetime, solution, alltypes ? LibrariesFlag.SolutionAndLibraries : LibrariesFlag.SolutionOnly, language, Shell.Instance.GetComponent<IShellLocks>(), Shell.Instance.GetComponent<IMainWindowPopupWindowContext>());

    // Фильтры скопированы "как есть"
    typeChooser.CompletionItemsPassFilter.Value = (Func<IDeclaredElement, bool>) (de =>
    {
      if (language.IsUnmanaged())
        return de.PresentationLanguage.Equals(language);
      return (de.PresentationLanguage.Equals(language) || de.PresentationLanguage.IsNullOrUnknown()) && (de is ITypeElement || de is ICompiledElement & alltypes) && (extraFilter == null || extraFilter(de));
    });

    typeChooser.PickerItemsPassFilter.Value = de =>
    {
        if (language.IsUnmanaged())
            return de.PresentationLanguage.Equals(language);
        if (!de.PresentationLanguage.Equals(language) && !de.PresentationLanguage.IsNullOrUnknown() || !(de is INamespace) && !(de is ITypeElement) && !(de is ICompiledElement & alltypes))
            return false;
        return extraFilter == null || extraFilter(de);
    };

    TypeNameService typeNameService = LanguageManager.Instance.TryGetService<TypeNameService>(language);

    // Конвертер фильтра скопирован "как есть", с использованием Range
    typeChooser.Settings.RealTextToCompletionItemsFilterConverter = (text, caretPosition) =>
    {
        TypeNameService typeNameService1 = typeNameService;
        // Используем тернарный оператор как в оригинале, никаких ?? с TextRange
        jb::System.Range range = typeNameService1 != null ? typeNameService1.GetRangeOfTypeNamePartAtPosition(text, caretPosition) : jb::System.Range.All;
        string str = text;
        jb::System.Index start = range.Start;
        int num = caretPosition;
        // Используем GetOffset, который есть у Index в этой сборке
        int offset = start.GetOffset(str.Length);
        return str.Substring(offset, num - offset);
    };

    // Инициализация. В лямбде - ЕДИНСТВЕННОЕ изменение логики.
    InitCompletionWithCustomTranslate(model, lifetime, typeChooser.Settings, component, (TranslateItemFunc) ((settings, item, text, caretPosition) =>
    {
      TypeNameService typeNameService2 = typeNameService;
      jb::System.Range range = typeNameService2 != null ? typeNameService2.GetRangeOfTypeNamePartAtPosition(text, caretPosition) : jb::System.Range.All;
      string str1 = text;

      // Вычисление префикса как в оригинале
      string str2 = str1.Substring(0, range.Start.GetOffset(str1.Length));

      // --- ИЗМЕНЕНИЕ ЗДЕСЬ ---
      // В оригинале: string str3 = CompletionBeTextBoxExtensions.TranslateItem(settings, item);
      // Мы берем ShortName напрямую, так как TranslateItem вернет FullName.
      string str3;
      if (item.Key is IDeclaredElement declaredElement)
      {
          str3 = declaredElement.ShortName;
      }
      else if (item.Key is DeclaredElementOccurrence occurrence && occurrence.GetDeclaredElement() is not null)
      {
          str3 = occurrence.GetDeclaredElement()!.ShortName;
      }
      else
      {
          str3 = item.DisplayName.Text;
      }
      // -----------------------

      string str4 = text;
      jb::System.Index end = range.End;
      int length = str4.Length;
      // Вычисление суффикса как в оригинале
      int offset = end.GetOffset(length);
      string str5 = str4.Substring(offset, length - offset);
      return str2 + str3 + str5;
    }));

    return model;
  }

    // ==========================================================================================
    // Ниже скопированные приватные методы из CompletionBeTextBoxExtensions.cs
    // ==========================================================================================

    private delegate string TranslateItemFunc(
        CompletionPickerSettings model,
        JetPopupMenuItem menuItem,
        string text,
        int caretPosition);

    private static void InitCompletionWithCustomTranslate(
        BeTextBox model,
        Lifetime lifetime,
        CompletionPickerSettings completionPickerSettings,
        IShellLocks shellLocks,
        TranslateItemFunc fTranslateItem)
    {
        var iconHost = Shell.Instance.GetComponent<IconHostBase>();
        completionPickerSettings.IsSelectingAllTextOnCompletion.Value = false;

        model.Settings.Caret.View(lifetime, (lt, caretPosition) =>
            shellLocks.ExecuteOrQueueReadLockEx(lt, "InitCompletionWithCustomTranslate::Validate", () =>
            {
                string text = model.GetText().NON_LOCALIZABLE();

                // Обновляем текст фильтрации
                completionPickerSettings.CompletionModel.Value.FilterText.Value =
                    GetFilterText(model, completionPickerSettings, caretPosition);

                // Обертка для функции перевода
                Func<JetPopupMenuItem, string> fTranslateItemCarried = item =>
                    fTranslateItem(completionPickerSettings, item, text, caretPosition);

                // Когда модель готова, обновляем список элементов для Rider
                completionPickerSettings.CompletionModel.Value.IsReady.WhenTrueOnce(lt, () =>
                    model.CompletionItems.Value = ToRiderList(
                        iconHost,
                        completionPickerSettings.CompletionModel.Value.Items.GroupBy(fTranslateItemCarried),
                        fTranslateItemCarried
                    ));
            }));
    }

    private static string GetFilterText(
        BeTextBox model,
        CompletionPickerSettings settings,
        int caretPosition)
    {
        string str = model.GetText().NON_LOCALIZABLE();
        if (caretPosition > str.Length)
            caretPosition = str.Length;

        return settings.RealTextToCompletionItemsFilterConverter == null
            ? str.Substring(0, caretPosition)
            : settings.RealTextToCompletionItemsFilterConverter(str, caretPosition).NON_LOCALIZABLE();
    }

    private static List<BeCompletionElement> ToRiderList(
        IconHostBase iconHost,
        IEnumerable<IGrouping<string, JetPopupMenuItem>> popupMenuItems,
        Func<JetPopupMenuItem, string> fTranslateItem)
    {
        // Логика группировки (если несколько элементов имеют одинаковый текст вставки)
        bool showRight = popupMenuItems.All(i => i.IsSingle());

        return popupMenuItems.Select(grouping =>
        {
            var jetPopupMenuItem = grouping.FirstOrDefault();
            if (jetPopupMenuItem == null) return null;

            // Обработка левого текста (DisplayName)
            BeAbstractText leftText = jetPopupMenuItem.DisplayName.GetBeRichText();
            if (jetPopupMenuItem.DisplayName.GetFormattedParts().All(p => p.Style.Equals(TextStyle.Default)))
                leftText = jetPopupMenuItem.DisplayName.Text.GetBeLabel();

            // Создание элемента completion
            BeCompletionElement riderItem;
            if (jetPopupMenuItem.Style != MenuItemStyle.None)
            {
                riderItem = new BeCompletionItem(
                    fTranslateItem(jetPopupMenuItem), // Здесь вызовется наша кастомная логика
                    iconHost.Transform(jetPopupMenuItem.Icon),
                    leftText);
            }
            else
            {
                riderItem = new BeCompletionNoActionItem(
                    iconHost.Transform(jetPopupMenuItem.Icon),
                    leftText);
            }

            // Добавление правой части (ShortcutText / Namespace)
            if (riderItem is BeCompletionItem completionItem)
            {
                var shortcutText = jetPopupMenuItem.ShortcutText;
                if (showRight && !shortcutText.IsNullOrEmpty())
                {
                    completionItem.RightIcon.Value = iconHost.Transform(jetPopupMenuItem.TailGlyph);
                    completionItem.RightText.Value = shortcutText.NON_LOCALIZABLE().GetBeRichText();
                }
            }
            return riderItem;
        })
        .Where(x => x != null)
        .ToList()!;
    }
}
