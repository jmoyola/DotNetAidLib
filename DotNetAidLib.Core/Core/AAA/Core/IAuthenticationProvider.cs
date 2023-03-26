using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.AAA.Core
{
    public interface IAuthenticationProvider : IDisposable
    {
        string ProviderType { get; }
        IEnumerable<IIdentity> GetIdentity(IDictionary<string, object> properties);
    }
}