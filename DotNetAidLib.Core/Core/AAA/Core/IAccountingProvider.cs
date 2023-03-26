using System.Collections.Generic;

namespace DotNetAidLib.Core.AAA.Core
{
    public interface IAccountingProvider
    {
        string ProviderType { get; }
        IList<IAccount> GetAccount(IIdentity identity, IResource resource, IDictionary<string, object> properties);
    }
}