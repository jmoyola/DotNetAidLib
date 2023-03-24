using System;
using System.Collections.Generic;

namespace Library.AAA.Core
{
    public interface IAuthorizationProvider : IDisposable
    {
        String ProviderType { get; }
        IList<IRole> GetRole(IIdentity identity, IResource resource, IDictionary<String, Object> properties);
    }
}
