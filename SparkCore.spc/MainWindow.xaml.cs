using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SparkCore.spc.ViewModels;
using SparkCore.spc.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SparkCore.spc;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public FileViewModel ViewModel
    {
        get;
    }
    public MainWindow()
    {
        this.InitializeComponent();
        ViewModel = new FileViewModel("hola", "");
    }

    private void codeText_TextChanged(object sender, RoutedEventArgs e)
    {
        codeText.Document.GetText(TextGetOptions.None, out var text);

        if (text == ViewModel.Text) return;
        ViewModel.Text = text;

        TokenText.IsReadOnly = false;
        SyntaxTreeText.IsReadOnly = false;
        IntermText.IsReadOnly = false;

        ViewModel.ChangeDisplay((RichEditBox)sender, TokenText, SyntaxTreeText, IntermText);
        TokenText.IsReadOnly = true;
        SyntaxTreeText.IsReadOnly = true;
        IntermText.IsReadOnly = true;
    }
    //private void NavigationViewItem_Tapped(object sender, TappedRoutedEventArgs e)
    //{
    //    var newTab = new FileTab();
    //    newTab.IconSource = new SymbolIconSource() { Symbol = Symbol.Document };
    //    newTab.Header = "New Document";
    //    newTab.Page.Navigate(typeof(FilePage));
    //    OpenFilesView.TabItems.Add(newTab);
    //}
}
