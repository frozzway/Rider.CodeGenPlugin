using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Threading;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.Util.Media;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.UI;
using Rider.Plugins.CodeGenJetbrains.UI.Components;

namespace Rider.Plugins.CodeGenJetbrains.Sandbox;

public static class SandboxElements
{
    public static BeGrid GetGrid(Lifetime lt, IDataContext context, IProjectFolder folder)
    {
        //var label = BeControls.BeLabel(AppDomain.CurrentDomain.BaseDirectory);
        var checkbox = BeControls.GetCheckBox("MyCheckBox", "checkbox1", lifetime: lt);
        var textBox = BeControls.GetTextBox(lt);
        var grid = BeControls.GetAutoGrid();
        var selectedItemLabel = BeControls.BeLabel("SelectedItem");
        var solution = folder.GetSolution();

        var dialogHost = context.GetComponent<IDialogHost>();

        var button = BeControls.GetButton("OpenModal".GetBeLabel(), onClick: () =>
        {
            dialogHost.ShowModal(lt, new ShellLocks(lt), "button", title: "Choose value", content: GetSimpleGrid, () => {});
        });

        var inputField1 = InputFieldFactory.CreateTextBox("InputField1", lt);
        var inputField2 = InputFieldFactory.CreateTextBox("InputField2", lt);

        grid.AddElements(textBox, checkbox, selectedItemLabel, button, inputField1.Control, inputField2.Control);
        grid.AddElement(BeControls.GetButton("Create SimpleAction.cs".GetBeLabel(), lt, () => CreateFileInFolder(folder)));
        grid.AddElement(BeControls.GetButton("Create SimpleFolder".GetBeLabel(), lt, () => CreateFolder(folder)));
        grid.AddElement(BeControls.GetButton("Debug".GetBeLabel(), lt, () => dialogHost.Show(CreateCheckboxListInDialog)));
        grid.AddElement(BeControls.GetTextBox(lt, id: $"textbox12345").WithTypeCompletionShort(folder.GetSolution(), lt, CSharpLanguage.Instance!));
        grid.AddElement(BeControls.GetTextBox(lt, id: $"textbox12346").WithEndpointsCompletion(folder.GetSolution(), lt));
        grid.AddElement(GetCheckBoxList(lt));

        var horizontalGrid = BeControls.GetEmptyGrid(GridOrientation.Horizontal);

        horizontalGrid
            .AddElement("BeHintSettings".GetBeLabel())
            .AddElement(BeControls.GetRichText("JetBrains.Rider.Model", fgColor: new JetRgbaColor(128, 128, 128, 255))
            );

        grid.AddElement(horizontalGrid);

        var form = new InputFieldsForm(lt, labelWidth: 120)
            .AddInputField(InputFieldFactory.CreateTextBox("FullName:", lt, "Enter your name"))
            .AddInputField(InputFieldFactory.CreateTextBox("FullEmail:" , lt, "example@email.com"))
            .AddInputField(InputFieldFactory.CreateTextBox("PhoneNum:", lt, "+7 (999) 123-45-67"));

        grid.AddElement(form.Grid);

        grid.AddElement(new InputFieldsForm(lt, labelWidth: 0)
            .AddInputField(InputFieldFactory.CreateTextBox("Class:", lt, "Enter your name"))
            .AddInputField(InputFieldFactory.CreateTextBox("Type:", lt, "example@email.com"))
            .Grid);

        return grid;
    }

    private static void Sandbox(Lifetime lifetime, IDataContext context)
    {
        var box = BeControls.GetTextBox(lifetime, id: $"textbox_");
        ISolution solution = default;
        var iconManager = solution.GetComponent<PsiIconManager>();
        var icon = iconManager.GetImage(CLRDeclaredElementType.METHOD);
        // box.WithFolderCompletion()
    }

    private static void CreateFileInFolder(IProjectFolder folder)
        => folder.CreateFileWithContent("SimpleClass.cs", "12345");

    private static void CreateFolder(IProjectFolder parentFolder)
        => parentFolder.CreateFolder("SimpleFolder");

    private record Item(string Name, int Index);

    private static BeTreeGrid GetCheckBoxList(Lifetime lifetime)
    {
        var selected = new HashSet<Item>();

        var config = new TreeConfiguration(
            columns:
            [
                ("name", new BeUnitSize(BeSizingType.Fit)),
                ("index", new BeUnitSize(BeSizingType.Fit))
            ]);


        string[] secondColumns = ["100", "200", "300", "some_some_some", "wowWowWow_wowWowWow_Wow", "1"];
        string[] rawItems = ["id", "c_date", "benefit_group_id_array", "long_some_text", "repeat_days_of_week_array", "registration_attempts_limit"];
        var comboBoxes = new BeComboBox[secondColumns.Length];
        var items = rawItems.Select((text, index) => new Item(text, index)).ToArray();

        var initialized = false;
        BeTreeGridExtensions.CheckedListLinePresentation<Item> presentation = (lt, element, properties) =>
        {
            if (!initialized)
                properties.Included.SetValue(true);
            if (selected.Count == items.Length)
                initialized = true;

            var comboBox = BeControls.GetComboBox(lt, secondColumns, selectedValue: secondColumns[element.Index]);
            comboBoxes[element.Index] = comboBox;

            return [element.Name.GetBeLabel(), comboBox];
        };

        var listEvents = items.ToListEvents("someId123456");
        var beGrid = listEvents.GetBeListWithCheckBoxes(lifetime, presentation, config);
        return beGrid;
    }

    private static BeDialog CreateCheckboxListInDialog(Lifetime lifetime)
    {
        var beGrid = GetCheckBoxList(lifetime);
        return beGrid.InDialog("title", "id123455677126", DialogModality.MODAL)
            .WithOkButton(lifetime, () => {})
            .WithCancelButton(lifetime);
    }

    private static BeGrid GetSimpleGrid(Lifetime lifetime)
    {
        var label = BeControls.BeLabel("MyLabel");
        var textBox = BeControls.GetTextBox(lifetime);
        var grid = BeControls.GetAutoGrid();
        grid.AddElements(label, textBox);
        return grid;
    }
}
