using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace Library.AAA.Core
{
    public class AuthenticationFactory: IAuthenticationProvider
    {
        private IList<IAuthenticationProvider> providers = new List<IAuthenticationProvider>();

        private Func<IAuthenticationProvider, Exception, bool> errorTryNext = null;
        private IDictionary<string, object> properties=null;

        public AuthenticationFactory()
            :this(new Dictionary<string, object>())
        { }

        public AuthenticationFactory(IDictionary<string, object> properties)
        {
            Assert.NotNull(properties, nameof(properties));

            this.properties = properties;
        }

        public String ProviderType { get { return "mixed"; } }

        public IList<IAuthenticationProvider> Providers {
            get { return this.providers; }
        }

        public Func<IAuthenticationProvider, Exception, bool> ErrorTryNext
        {
            get
            {
                return errorTryNext;
            }

            set
            {
                errorTryNext = value;
            }
        }

        public IDictionary<string, object> Properties {
            get { return properties; }
        }

        public void Dispose ()
        {
            this.providers.ToList().ForEach(v=>v.Dispose());
        }

        public IEnumerable<IIdentity> GetIdentity() {
            return this.GetIdentity(this.properties);
        }

        public IEnumerable<IIdentity> GetIdentity(IDictionary<string, object> properties) {
            return GetIdentity(properties, this.Providers);
        }

        public IEnumerable<IIdentity> GetIdentity(IDictionary<string, object> properties, IList<IAuthenticationProvider> providers)
        {
            List<IIdentity> ret = new List<IIdentity>();

            foreach (IAuthenticationProvider provider in providers) {
                try {
                    IEnumerable<IIdentity> identities = provider.GetIdentity(properties);
                    if(identities!=null)
                        ret.AddRange(identities);
                }
                catch (Exception ex){
                    if (this.ErrorTryNext==null)
                        throw ex;
                    else {
                        if (!this.errorTryNext.Invoke(provider, ex))
                            break;
                    }
                }
            }
            return ret;
        }
    }
}
