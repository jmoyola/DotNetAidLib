using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Cmd
{
    public class ParameterInfoFactory : List<ParameterInfo>
    {
        private static readonly Regex r = new Regex(@"^(([-/]{1,2})([^\s=:]+))?([=:]?\s?(.*))$");

        public IList<ParameterInfoValue> Values { get; } = new List<ParameterInfoValue>();

        public void Process(string[] args, bool includeOptional)
        {
            IList<KeyValuePair<string, string>> kvl = new List<KeyValuePair<string, string>>();
            // Grupo 1?=--/
            // Grupo 3?=name
            // Grupo 5=value
            for (var i = 0; i < args.Length; i++)
            {
                var m = r.Match(args[i]);

                if (!m.Success)
                    throw new ParameterInfoException("Malformed argument: " + args[i]);

                kvl.Add(new KeyValuePair<string, string>(m.Groups[3].Value, m.Groups[5].Value));
            }

            Process(kvl, includeOptional);
        }

        public void Process(IDictionary<string, string> args, bool includeOptional)
        {
            Process(args, includeOptional);
        }

        public void Process(IList<KeyValuePair<string, string>> args, bool includeOptional)
        {
            Values.Clear();

            for (var i = 0; i < args.Count; i++)
            {
                var kv = args[i];
                var name = kv.Key;
                var value = kv.Value;

                ParameterInfo validArg = null;

                // Si tiene nombre
                if (!string.IsNullOrEmpty(name))
                {
                    validArg = this
                        .FirstOrDefault(v =>
                            typeof(NamedParameterInfo).IsAssignableFrom(v.GetType()) &&
                            ((NamedParameterInfo) v).Name == name);
                    if (validArg == null)
                        throw new ParameterInfoException("Argument name '" + name + "' is unknow.");
                }
                // Si no tiene nombre
                else
                {
                    validArg = this
                        .FirstOrDefault(v =>
                            typeof(IndexParameterInfo).IsAssignableFrom(v.GetType()) &&
                            ((IndexParameterInfo) v).Index == i);
                    if (validArg == null)
                        throw new ParameterInfoException("Argument value '" + value + "' is unknow.");
                }

                Values.Add(new ParameterInfoValue(validArg, value));
            }

            // Buscamos argumentos no opcionales que faltan
            var missing = this.Where(v => !v.Optional && !Values.Any(r => v.Equals(r.Parameter)));
            if (missing.Count() > 0)
                throw new ParameterInfoException("Argument/s '" + missing.Select(v => v.Name).ToStringJoin(", ") +
                                                 "' is missing.");

            // Si se incluye opcionales, se añaden con valores por defecto.
            if (includeOptional)
                foreach (var validArg in this.Where(v => v.Optional
                                                         && !Values.Any(r => v.Equals(r.Parameter))))
                    Values.Add(new ParameterInfoValue(validArg, validArg.DefaultValue));
        }

        public ParameterInfoValue GetValue(string parameterName)
        {
            var pi = Values.FirstOrDefault(v => v.Parameter.Name.Equals(parameterName));
            if (pi == null)
                throw new ParameterInfoException("Don't exists parameter '" + parameterName + "'.");

            return pi;
        }

        public IEnumerable<ParameterInfoValue> GetValues(string parameterName)
        {
            return Values.Where(v => v.Parameter.Name.Equals(parameterName));
        }
    }
}