using System;
using System.Collections.Generic;
using DotNetAidLib.Core.AAA.Core;

namespace DotNetAidLib.Core.AAA.Imp
{
    public class EnvVarAuthUserPasswordProvider : IAuthenticationProvider
    {
        private static Dictionary<string, EnvVarAuthUserPasswordProvider> instances =
            new Dictionary<string, EnvVarAuthUserPasswordProvider>();

        public string ProviderType => "UserPassword";

        public void Dispose()
        {
        }

        public IEnumerable<IIdentity> GetIdentity(IDictionary<string, object> properties)
        {
            UserPasswordIdentity ret = null;

            var variableName = "ENVVAR_AUTH_USERPASSWORD";

            if (properties != null && properties.ContainsKey("ENVVAR_AUTH_USERPASSWORD"))
                variableName = properties["ENVVAR_AUTH_USERPASSWORD"].ToString();

            var variableValue = Environment.GetEnvironmentVariable(variableName);
            if (UserPasswordIdentity.TryParse(variableValue, out ret))
                return new IIdentity[] {ret};
            return new IIdentity[] { };
        }
    }
}