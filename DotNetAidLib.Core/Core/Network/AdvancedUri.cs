using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Network
{
    public class AdvancedUri : Uri
    {
        public AdvancedUri(Uri uri)
            : this(uri.ToString())
        {
        }

        public AdvancedUri(AdvancedUri advanceUri)
            : this(advanceUri.ToString())
        {
            IsRemovable = advanceUri.IsRemovable;
        }

        public AdvancedUri(string sUri)
            : base(ProcessNewUri(sUri))
        {
            if (sUri.StartsWith("removable", StringComparison.InvariantCultureIgnoreCase))
                IsRemovable = true;
        }

        public bool IsRemovable { get; }

        private static string ProcessNewUri(string sUri)
        {
            if (sUri.StartsWith("removable", StringComparison.InvariantCultureIgnoreCase))
                return sUri.Replace("removable", "file");
            return sUri;
        }

        public static IList<AdvancedUri> FromString(string sUriList)
        {
            IList<AdvancedUri> ret = new List<AdvancedUri>();

            foreach (var sUri in sUriList.Split(';', ','))
                ret.Add(new AdvancedUri(sUri));

            return ret;
        }

        public static bool TryParse(string sUri, ref AdvancedUri Uri)
        {
            try
            {
                Uri = new AdvancedUri(sUri);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryParse(string sUriList, ref IList<AdvancedUri> Uris)
        {
            try
            {
                Uris = FromString(sUriList);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public AdvancedUri ToUri(string userName, string password)
        {
            var ub = new UriBuilder(this);
            ub.UserName = userName;
            ub.Password = password;
            return new AdvancedUri(ub.Uri.ToString());
        }

        public AdvancedUri ToUri(string scheme, string host, int port)
        {
            var ub = new UriBuilder(this);
            ub.Scheme = scheme;
            ub.Host = host;
            ub.Port = port;
            return new AdvancedUri(ub.Uri.ToString());
        }

        public override bool Equals(object comparand)
        {
            if (comparand == null)
                return false;

            return typeof(Uri).IsAssignableFrom(comparand.GetType())
                   && ToString().Equals(comparand.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}