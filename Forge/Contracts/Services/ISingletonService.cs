using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.Contracts.Services;
internal interface ISingletonService<T>
{
    private static readonly Lazy<T> lazy = new(() => (T)new object());
    public static T Instance => lazy.Value;
}
