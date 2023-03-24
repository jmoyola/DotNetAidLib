using System;
using System.Collections.Generic;

namespace Library.AAA.Core
{
    public interface IAccountingProvider
    {
        String ProviderType { get; }
        IList<IAccount> GetAccount(IIdentity identity, IResource resource, IDictionary<String, Object> properties);
    }
}
