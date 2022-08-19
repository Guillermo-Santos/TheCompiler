using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forge.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Forge.Helpers;
public sealed class ProjectFileTemplateSelector : DataTemplateSelector
{
    public DataTemplate FolderTemplate
    {
        get; set;
    }
    public DataTemplate DocumentTemplate
    {
        get; set;
    }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            Folder => FolderTemplate,
            _ => DocumentTemplate,
        };
    }
}
