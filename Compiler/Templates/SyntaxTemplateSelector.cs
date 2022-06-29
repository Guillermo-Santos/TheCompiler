using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Compiler.Templates
{
    public class SyntaxTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BasicTemplate { get; set; }
        public DataTemplate ChildTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var explorerItem = (SyntaxTemplate)item;
            return explorerItem.Type == SyntaxTemplate.SyntaxItemType.Basic ? BasicTemplate : ChildTemplate;
        }
    }
}
