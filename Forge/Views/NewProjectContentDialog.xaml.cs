using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Forge.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Forge.Views;
public sealed partial class NewProjectContentDialog : ContentDialog
{
    readonly NewProjectViewModel ViewModel;
    public NewProjectContentDialog()
    {
        InitializeComponent();
        ViewModel = new();
    }

    private void ProjectName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        var path = $"{ViewModel.RootPath}\\{ProjectName.Text}";
        ViewModel.ProjectPath = path;
        var a = 12;
    }

    private void ProjectName_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        var path = $"{ViewModel.RootPath}\\{ProjectName.Text}";
        ViewModel.ProjectPath = path;
        var a = 12;
    }
}
