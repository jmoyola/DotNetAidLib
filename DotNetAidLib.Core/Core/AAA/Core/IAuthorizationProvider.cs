using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.AAA.Core
{
    public interface IAuthorizationProvider : IDisposable
    {
        string ProviderType { get; }
        IList<IRole> GetRole(IIdentity identity, IResource resource, IDictionary<string, object> properties);
    }
}