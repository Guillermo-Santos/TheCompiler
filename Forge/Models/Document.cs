using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SparkCore.IO.Text;

namespace Forge.Models;
public sealed class Document : ObservableObject
{
    public string FileName
    {
        get;
        set;
    }
    private string text;
    public string Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }
    public DependencyProperty TextProperty = DependencyProperty.Register(
                                            nameof(TextProperty),
                                            typeof(string),
                                            typeof(Document),
                                            new PropertyMetadata(default(string))
                                            );
    public Document(string filename, string text)
    {
        FileName = filename;
        Text = text;
    }
    public SourceText SourceText => SourceText.From(Text, FileName);
}
