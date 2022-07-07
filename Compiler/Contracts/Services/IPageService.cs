using System;

namespace Compiler.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);
}
