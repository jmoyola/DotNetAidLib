using System;
using Library.AAA.Core;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;

namespace Library.AAA.Imp
{
    public class UserPasswordIdentity:IIdentity
    {
        protected String userName;
        protected String password;
        protected String domain;

        public UserPasswordIdentity(String userName)
        : this(userName, null, null) { }

        public UserPasswordIdentity(String userName, String password)
        :this(userName, password, null) {}

        public UserPasswordIdentity(String userName, String password, String domain)
        {
            Assert.NotNullOrEmpty(userName, nameof(userName));

            this.userName = userName;
            this.password = password;
            this.domain = domain;
        }

        public string Id {
            get {
                return this.userName               
                + this.domain.IfNotNullOrEmpty( "@" + this.domain);
            }
        }

        public string UserName
        {
            get
            {
                return userName;
            }
        }
        public string Password
        {
            get
            {
                return password;
            }
        }
        public string Domain
        {
            get
            {
                return domain;
            }
        }

        public override string ToString()
        {
            return this.userName
            + this.domain.IfNotNullOrEmpty("@" + this.domain);

        }

        private static Regex userPasswordRegex = new Regex(@"^([^:@\s]+)(:([^@\s]+))?(@([^\s]+))?$");
        public static UserPasswordIdentity Parse(String value) {
            Match m = userPasswordRegex.Match(value);

            if (!m.Success)
                throw new InvalidCastException("Value '" +  value + "' can't not parse to UserPassword identity '<userName>[:password][@<domain>]'");

            return new UserPasswordIdentity(
                m.Groups[1].Value,
                (String.IsNullOrEmpty(m.Groups[3].Value)?null:m.Groups[3].Value),
                (String.IsNullOrEmpty(m.Groups[5].Value)?null:m.Groups[5].Value)
            );
        }

        public override bool Equals(object obj)
        {
            if(obj!=null && typeof(UserPasswordIdentity).IsAssignableFrom(obj.GetType())) {
                UserPasswordIdentity o = (UserPasswordIdentity)obj;
                return this.userName.Equals(o.UserName, StringComparison.CurrentCultureIgnoreCase)
                    && this.domain.Equals(o.Domain, StringComparison.CurrentCulture);
            }
            else
                return this.Id.Equals(((IIdentity)obj).Id);
        }

        public override int GetHashCode()
        {
            return this.UserName.ToUpper().GetHashCode()
                + this.Domain.ToUpper().GetHashCode();

        }
        public static bool TryParse(String value, out UserPasswordIdentity o) {
            o = null;
            try {
                o = UserPasswordIdentity.Parse(value);
                return true;
            }
            catch {
                return false;
            }
        }
    }
}
