using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cmd
{
    public class ParameterInfoFactory: List<ParameterInfo>
    {
        private static Regex r = new Regex (@"^(([-/]{1,2})([^\s=:]+))?([=:]?\s?(.*))$");
        private IList<ParameterInfoValue> values = new List<ParameterInfoValue>();

        public IList<ParameterInfoValue> Values { get => this.values;}

        public ParameterInfoFactory ()
        {
        }

        public void Process(String [] args, bool includeOptional)
        {
            IList<KeyValuePair<String, String>> kvl = new List<KeyValuePair<String, String>> ();
            // Grupo 1?=--/
            // Grupo 3?=name
            // Grupo 5=value
            for (int i = 0; i < args.Length; i++) {
                Match m = r.Match (args [i]);

                if (!m.Success)
                    throw new ParameterInfoException ("Malformed argument: " + args [i]);

                kvl.Add (new KeyValuePair<String, String>(m.Groups [3].Value, m.Groups [5].Value));
            }

            this.Process (kvl, includeOptional);
        }

        public void Process (IDictionary<String, String> args, bool includeOptional){
            this.Process (args, includeOptional);
        }

        public void Process(IList<KeyValuePair<String, String>> args, bool includeOptional) {
            this.values.Clear();

            for (int i=0; i< args.Count; i++){
                KeyValuePair<String, String> kv = args [i];
                String name = kv.Key;
                String value = kv.Value;

                ParameterInfo validArg = null;

                // Si tiene nombre
                if (!String.IsNullOrEmpty (name)) {
                    validArg = this
                    .FirstOrDefault (v => typeof(NamedParameterInfo).IsAssignableFrom(v.GetType()) && ((NamedParameterInfo)v).Name == name);
                    if (validArg == null)
                        throw new ParameterInfoException ("Argument name '" + name + "' is unknow.");
                }
                // Si no tiene nombre
                else {
                    validArg = this
                    .FirstOrDefault (v => typeof(IndexParameterInfo).IsAssignableFrom(v.GetType()) && ((IndexParameterInfo)v).Index == i);
                    if (validArg == null)
                        throw new ParameterInfoException ("Argument value '" + value + "' is unknow.");
                }
                this.values.Add (new ParameterInfoValue(validArg, value));
            }

            // Buscamos argumentos no opcionales que faltan
            IEnumerable<ParameterInfo> missing = this.Where(v => !v.Optional && !this.values.Any (r => v.Equals (r.Parameter)));
            if (missing.Count()>0)
                throw new ParameterInfoException ("Argument/s '" + missing.Select(v=> v.Name).ToStringJoin(", ") + "' is missing.");

            // Si se incluye opcionales, se añaden con valores por defecto.
            if (includeOptional) {
                foreach (ParameterInfo validArg in this.Where (v => v.Optional
                       && !this.values.Any (r => v.Equals (r.Parameter)))){
                    this.values.Add (new ParameterInfoValue(validArg, validArg.DefaultValue));
                }
            }
        }

        public ParameterInfoValue GetValue(String parameterName) {
            ParameterInfoValue pi = this.values.FirstOrDefault(v=>v.Parameter.Name.Equals(parameterName));
            if (pi == null)
                throw new ParameterInfoException("Don't exists parameter '" + parameterName + "'.");

            return pi;
        }

        public IEnumerable<ParameterInfoValue> GetValues(String parameterName){
            return this.values.Where(v => v.Parameter.Name.Equals(parameterName));
        }
    }
}
