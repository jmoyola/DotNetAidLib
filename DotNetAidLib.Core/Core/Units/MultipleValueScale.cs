using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Units
{
    public class MultipleValueScale
    {
        private static readonly string _Pattern = @"(([a-zA-Z/]+)\s*)?(\d+[\.,]?\d*)(\s*([a-zA-Z/]+))?";
        private string _Multiple = "";
        private readonly IDictionary<string, decimal> multiples = new Dictionary<string, decimal>();

        public MultipleValueScale(string multipleList, decimal pow, int startPultiple, decimal decimalValue,
            string multiple)
            : this(multipleList, pow, startPultiple)
        {
            DecimalValue = decimalValue;
            Multiple = multiple;
        }

        public MultipleValueScale(string multipleList, decimal pow, int startPultiple, string value)
            : this(multipleList, pow, startPultiple)
        {
            Value = value;
        }

        public MultipleValueScale(string multipleList, decimal pow, int startPultiple)
        {
            var aMultipleList = multipleList.Split(',', ';');
            for (var i = startPultiple; i < aMultipleList.Length; i++)
            {
                var multiple = aMultipleList[i].Trim().Split('|');
                foreach (var m in multiple)
                    multiples.Add(new KeyValuePair<string, decimal>(m.Trim(), i * pow));
            }
        }

        public MultipleValueScale()
        {
            DecimalValue = 0;
            Multiple = "";
        }

        public MultipleValueScale(decimal decimalValue, string multiple)
        {
            DecimalValue = decimalValue;
            Multiple = multiple;
        }

        public MultipleValueScale(string value)
        {
            Value = value;
        }

        public virtual IDictionary<string, decimal> Multiples => multiples;

        public decimal DecimalValue { get; set; }

        public string Multiple
        {
            get => _Multiple;
            set
            {
                if (!Multiples.ContainsKeyIgnoreCase(value))
                    throw new Exception("Multiple '" + value + "' is not valid. Valid multiples are only: " +
                                        Multiples.Keys.ToStringJoin(", ") + ".");
                _Multiple = value;
            }
        }

        public string Value
        {
            get => DecimalValue + (string.IsNullOrEmpty(Multiple) ? "" : " " + Multiple);
            set
            {
                var r = new Regex(_Pattern);
                var m = r.Match(value);
                if (!m.Success)
                    throw new Exception("No valid value.");
                DecimalValue = decimal.Parse(m.Groups[3].Value);

                if (!string.IsNullOrEmpty(m.Groups[2].Value))
                    Multiple = m.Groups[2].Value;
                else if (!string.IsNullOrEmpty(m.Groups[5].Value))
                    Multiple = m.Groups[5].Value;
                else
                    throw new Exception("Missing multiple.");
            }
        }

        public decimal TotalValue => DecimalValue * decimal.Parse(Multiples.GetValueIgnoreCase(_Multiple).ToString());

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && GetType().IsAssignableFrom(obj.GetType()))
                return TotalValue.Equals(((MultipleValueScale) obj).TotalValue);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return TotalValue.GetHashCode();
        }

        public static implicit operator decimal(MultipleValueScale v)
        {
            return v.TotalValue;
        }
    }
}