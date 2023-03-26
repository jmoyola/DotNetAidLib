using System;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.AAA.Core;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.AAA.Imp
{
    public class UserPasswordIdentity : IIdentity
    {
        private static readonly Regex userPasswordRegex = new Regex(@"^([^:@\s]+)(:([^@\s]+))?(@([^\s]+))?$");
        protected string domain;
        protected string password;
        protected string userName;

        public UserPasswordIdentity(string userName)
            : this(userName, null, null)
        {
        }

        public UserPasswordIdentity(string userName, string password)
            : this(userName, password, null)
        {
        }

        public UserPasswordIdentity(string userName, string password, string domain)
        {
            Assert.NotNullOrEmpty(userName, nameof(userName));

            this.userName = userName;
            this.password = password;
            this.domain = domain;
        }

        public string UserName => userName;

        public string Password => password;

        public string Domain => domain;

        public string Id =>
            userName
            + domain.IfNotNullOrEmpty("@" + domain);

        public override string ToString()
        {
            return userName
                   + domain.IfNotNullOrEmpty("@" + domain);
        }

        public static UserPasswordIdentity Parse(string value)
        {
            var m = userPasswordRegex.Match(value);

            if (!m.Success)
                throw new InvalidCastException("Value '" + value +
                                               "' can't not parse to UserPassword identity '<userName>[:password][@<domain>]'");

            return new UserPasswordIdentity(
                m.Groups[1].Value,
                string.IsNullOrEmpty(m.Groups[3].Value) ? null : m.Groups[3].Value,
                string.IsNullOrEmpty(m.Groups[5].Value) ? null : m.Groups[5].Value
            );
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(UserPasswordIdentity).IsAssignableFrom(obj.GetType()))
            {
                var o = (UserPasswordIdentity) obj;
                return userName.Equals(o.UserName, StringComparison.CurrentCultureIgnoreCase)
                       && domain.Equals(o.Domain, StringComparison.CurrentCulture);
            }

            return Id.Equals(((IIdentity) obj).Id);
        }

        public override int GetHashCode()
        {
            return UserName.ToUpper().GetHashCode()
                   + Domain.ToUpper().GetHashCode();
        }

        public static bool TryParse(string value, out UserPasswordIdentity o)
        {
            o = null;
            try
            {
                o = Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}