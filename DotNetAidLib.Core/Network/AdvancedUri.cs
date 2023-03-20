using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Network
{
	public class AdvancedUri:Uri
	{
		private bool _IsRemovable = false;

		public AdvancedUri(Uri uri)
			:this(uri.ToString()){}

		public AdvancedUri(AdvancedUri advanceUri)
			:this(advanceUri.ToString()){
			this._IsRemovable = advanceUri.IsRemovable;
		}
		
		public AdvancedUri(String sUri)
			:base(ProcessNewUri(sUri))
		{
			if (sUri.StartsWith ("removable", StringComparison.InvariantCultureIgnoreCase))
				this._IsRemovable = true;
		}

		public bool IsRemovable{
			get{ return _IsRemovable;}
		}

		private static String ProcessNewUri(String sUri){
			if (sUri.StartsWith ("removable", StringComparison.InvariantCultureIgnoreCase))
				return sUri.Replace("removable", "file");
			else
				return sUri;
		}

		public static IList<AdvancedUri> FromString(String sUriList){

			IList<AdvancedUri> ret = new List<AdvancedUri>();

			foreach (String sUri in sUriList.Split (new char[]{';',','}))
				ret.Add (new AdvancedUri(sUri));

			return ret;
		}

		public static bool TryParse(string sUri, ref AdvancedUri Uri)
		{
			try{
				Uri=new AdvancedUri(sUri);
				return true;

			}catch{
				return false;
			}
		}

		public static bool TryParse(string  sUriList, ref IList<AdvancedUri> Uris)
		{
			try{
				Uris=AdvancedUri.FromString(sUriList);
				return true;

			}catch{
				return false;
			}
		}

        public AdvancedUri ToUri(String userName, String password) {
            UriBuilder ub = new UriBuilder(this);
            ub.UserName = userName;
            ub.Password = password;
            return new AdvancedUri(ub.Uri.ToString());
        }

        public AdvancedUri ToUri(String scheme, String host, int port)
        {
            UriBuilder ub = new UriBuilder(this);
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
                && this.ToString().Equals(comparand.ToString());
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}

