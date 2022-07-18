using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SparkCore.spc.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SparkCore.spc.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FilePage : Page
{
    public FileViewModel ViewModel
    {
        get;
    }
    public FilePage()
    {
        this.InitializeComponent();
        //ViewModel = new FileViewModel("hola", "");
    }
    public FilePage(string fileName, string text)
    {
        this.InitializeComponent();
        //ViewModel = new FileViewModel(fileName, text);
    }

    private void codeText_TextChanged(object sender, RoutedEventArgs e)
    {
        codeText.Document.GetText(TextGetOptions.None, out var text);
        if (text == ViewModel.Text) return;
        //ViewModel.Text = text;
        //ViewModel.ChangeDisplay((RichEditBox)sender);
    }
}
