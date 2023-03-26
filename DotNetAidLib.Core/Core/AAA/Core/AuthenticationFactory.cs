using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.AAA.Core
{
    public class AuthenticationFactory : IAuthenticationProvider
    {
        public AuthenticationFactory()
            : this(new Dictionary<string, object>())
        {
        }

        public AuthenticationFactory(IDictionary<string, object> properties)
        {
            Assert.NotNull(properties, nameof(properties));

            Properties = properties;
        }

        public IList<IAuthenticationProvider> Providers { get; } = new List<IAuthenticationProvider>();

        public Func<IAuthenticationProvider, Exception, bool> ErrorTryNext { get; set; } = null;

        public IDictionary<string, object> Properties { get; }

        public string ProviderType => "mixed";

        public void Dispose()
        {
            Providers.ToList().ForEach(v => v.Dispose());
        }

        public IEnumerable<IIdentity> GetIdentity(IDictionary<string, object> properties)
        {
            return GetIdentity(properties, Providers);
        }

        public IEnumerable<IIdentity> GetIdentity()
        {
            return GetIdentity(Properties);
        }

        public IEnumerable<IIdentity> GetIdentity(IDictionary<string, object> properties,
            IList<IAuthenticationProvider> providers)
        {
            var ret = new List<IIdentity>();

            foreach (var provider in providers)
                try
                {
                    var identities = provider.GetIdentity(properties);
                    if (identities != null)
                        ret.AddRange(identities);
                }
                catch (Exception ex)
                {
                    if (ErrorTryNext == null)
                    {
                        throw ex;
                    }

                    if (!ErrorTryNext.Invoke(provider, ex))
                        break;
                }

            return ret;
        }
    }
}