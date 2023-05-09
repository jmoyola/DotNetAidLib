using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Enums;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Core.Text
{
    public class PlaceHolderReplaceItem
    {
        private readonly String _name;
        private readonly Func<String, String> _replaceKeyFunction;

        public PlaceHolderReplaceItem(String name, Func<String, String> replaceKeyFunction)
        {
            _name=Assert.NotNullOrEmpty(name, nameof(name));
            _replaceKeyFunction=Assert.NotNull(replaceKeyFunction, nameof(replaceKeyFunction));
        }
        
        public String Name=>_name;
        public Func<String, String> ReplaceKeyFunction=>_replaceKeyFunction;
    }
    
    public class PlaceHolder
    {
        private readonly Regex _phRegex = new Regex(@"\$\{([^\:\}]+)(:([^\}]))?\}", RegexOptions.IgnoreCase);
        
        private static readonly IList<PlaceHolderReplaceItem> _commonReplaceItems;
        
        private readonly IList<PlaceHolderReplaceItem> _replaceItems;

        static PlaceHolder()
        {
            _commonReplaceItems = new List<PlaceHolderReplaceItem>();
            _commonReplaceItems.Add(new PlaceHolderReplaceItem("env", s => Environment.GetEnvironmentVariable(s)));
            _commonReplaceItems.Add(new PlaceHolderReplaceItem("date", s => String.IsNullOrEmpty(s)?DateTime.Now.ToString(CultureInfo.CurrentCulture):DateTime.Now.ToString(s)));
            _commonReplaceItems.Add(new PlaceHolderReplaceItem("pathSeparator", _ => Path.PathSeparator+""));
            _commonReplaceItems.Add(new PlaceHolderReplaceItem("newLine", _ => Environment.NewLine));
            _commonReplaceItems.Add(new PlaceHolderReplaceItem("username", _ => Environment.UserName));
            _commonReplaceItems.Add(new PlaceHolderReplaceItem("cwd", _ => Environment.CurrentDirectory));
            _commonReplaceItems.Add(new PlaceHolderReplaceItem("fileLocation", s =>
            {
                try
                {
                    FileLocation fl= s.ToEnum<FileLocation>(true);
                    return FileLocations.GetLocation(fl).FullName + Path.DirectorySeparatorChar;
                }
                catch
                {
                    return "!ERROR";
                }
            }));
            _commonReplaceItems.Add(new PlaceHolderReplaceItem("specialFolder", p =>
            {
                try
                {
                    Environment.SpecialFolder sf = p.ToEnum<Environment.SpecialFolder>(true);
                    return Environment.GetFolderPath(sf) + Path.DirectorySeparatorChar;
                }
                catch
                {
                    return "!ERROR";
                }
            }));
            _commonReplaceItems = _commonReplaceItems.ToList().AsReadOnly();
        }

        public static IList<PlaceHolderReplaceItem> CommonrReplaceItems=>_commonReplaceItems;
        
        public PlaceHolder()
        {
            this._replaceItems = new List<PlaceHolderReplaceItem>(_commonReplaceItems);
        }

        public PlaceHolder(IList<PlaceHolderReplaceItem> replaceItems)
        {
            Assert.NotNull(replaceItems, nameof(replaceItems));
            
            this._replaceItems = new List<PlaceHolderReplaceItem>(_commonReplaceItems);
            ((List<PlaceHolderReplaceItem>)this._replaceItems).AddRange(replaceItems);
        }
        
        public String Format(String value)
        {
            MatchCollection mc = _phRegex.Matches(value);
            foreach (Match m in mc)
            {
                PlaceHolderReplaceItem phri = _replaceItems.FirstOrDefault(v =>
                    v.Name.Equals(m.Groups[1].Value, StringComparison.InvariantCultureIgnoreCase));

                if (phri != null)
                    value = value.Replace(m.Value, phri.ReplaceKeyFunction(m.Groups[3].Value));
            }

            return value;
        }

        private static PlaceHolder _instance = new PlaceHolder();
        public static PlaceHolder Instance
        {
            get => _instance;
        }

        

        
    }
}