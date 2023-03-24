using System;
using System.Collections.Generic;

namespace Library.AAA.Core
{
    public interface IAuthenticationProvider: IDisposable
    {
        String ProviderType { get; }
        IEnumerable<IIdentity> GetIdentity(IDictionary<String, Object> properties);
    }
}
