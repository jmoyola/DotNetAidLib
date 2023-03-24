using System;
using Library.AAA.Core;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Library.AAA.Imp
{
    public class EnvVarAuthUserPasswordProvider : IAuthenticationProvider
    {
        private static Dictionary<String, EnvVarAuthUserPasswordProvider> instances = new Dictionary<string, EnvVarAuthUserPasswordProvider>();

        public EnvVarAuthUserPasswordProvider(){}

        public string ProviderType
        {
            get
            {
                return "UserPassword";
            }
        }

        public void Dispose()
        {

        }

        public IEnumerable<IIdentity> GetIdentity(IDictionary<string, object> properties)
        {
            UserPasswordIdentity ret = null;

            String variableName = "ENVVAR_AUTH_USERPASSWORD";

            if (properties != null && properties.ContainsKey("ENVVAR_AUTH_USERPASSWORD"))
                variableName = properties["ENVVAR_AUTH_USERPASSWORD"].ToString();

            String variableValue = Environment.GetEnvironmentVariable(variableName);
            if (UserPasswordIdentity.TryParse(variableValue, out ret))
                return new IIdentity[] { ret };
            else
                return new IIdentity[] {};
        }
    }
}