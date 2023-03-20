using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DotNetAidLib.Core.Time
{
    public static class TimeHelpers
    {
        public static String ToStringISO8601(this DateTime v)
        {
            return v.ToStringISO8601(false);
        }

        public static String ToStringISO8601(this DateTime v, bool includeMicroSeconds)
        {
            return v.ToStringISO8601(includeMicroSeconds, false);
        }

        public static String ToStringISO8601(this DateTime v, bool includeMicroSeconds, bool extended)
        {
            String format = null;

            if(extended)
                format="yyyy-MM-ddTHH:mm:ss" + (includeMicroSeconds ? ",fff":"");
            else
                format="yyyyMMddHHmmss" + (includeMicroSeconds ? "fff":"");

            return v.ToString(format, CultureInfo.InvariantCulture.DateTimeFormat);
        }

        public static String ToStringISO8601(this DateTimeOffset v) {
            return v.ToStringISO8601(false);
        }

        public static String ToStringISO8601(this DateTimeOffset v, bool includeMicroSeconds)
        {
            return v.ToStringISO8601(includeMicroSeconds, false);
        }

        public static String ToStringISO8601(this DateTimeOffset v, bool includeMicroSeconds, bool extended)
        {
            String ret = v.DateTime.ToStringISO8601(includeMicroSeconds, extended);

            if (TimeSpan.Zero.Equals(v.Offset))
                return ret+= "Z";
            else
                return ret+= (v.Offset.Hours>0?"+":"-") + Math.Abs(v.Offset.Hours).ToString().PadLeft(2,'0') + (extended?":":"") + v.Offset.Seconds.ToString().PadLeft(2,'0');
        }

        public static String ToStringISO8601(this TimeSpan v, bool includeMicroSeconds, bool extended)
        {
            String ret = "P";

            ret += SToStringISO8601(v.Days, 'D', extended);
            ret += "T";
            ret += SToStringISO8601(v.Hours, 'H', extended);
            ret += SToStringISO8601(v.Minutes, 'M', extended);
            ret += SToStringISO8601(v.Seconds, 'S', extended);
            ret += SToStringISO8601 (v.Milliseconds, 'F', extended);
            return ret;
        }

        private static String SToStringISO8601(int value, char sufix, bool extended) {
            String ret = null;

            if(value>0 || extended)
                ret = value.ToString() + sufix;

            return ret;
        }

        public static TimeSpan ToTimeSpanISO8601(this String v)
        {
            TimeSpan ret = TimeSpan.Zero;


            IDictionary<String, Decimal> timeSeconds = new Dictionary<String, Decimal>()
            { {"H",365M }, {"M",30M}, {"S",1M } };

            Regex r = new Regex(@"P(\d+(,\d+)?[YMWD]){0,4}(T(\d+(,\d+)?[HMSF]){0,3})?");
            Match m = r.Match(v);
            if (m.Success)
            {
                foreach (Capture c in m.Groups[1].Captures)
                    ret = ret.Add(ToTimeSpanISO8601Date(c.Value));

                foreach (Capture c in m.Groups[4].Captures)
                    ret = ret.Add(ToTimeSpanISO8601Time(c.Value));

                return ret;
            }
            else
                throw new InvalidCastException("No valid Iso8601 interval (P[nn[.nn]Y][nn[.nn]M][nn[.nn]W][nn[.nn]D]T[nn[.nn]H][nn[.nn]M][nn[.nn]S][nn[.nn]F])");
        }

        private static TimeSpan ToTimeSpanISO8601Date(String v) {
            IDictionary<String, Double> dateDays = new Dictionary<String, Double>()
            { {"Y",365D }, {"M",30D}, {"W",7D}, {"D",1D } };

            v=v.Replace(",", ".");
            IList<String> n = v.RegexGroupsMatches(@"(\d+(\.\d+)?)(\D)");

            return TimeSpan.FromDays(Double.Parse(n[1], CultureInfo.InvariantCulture.NumberFormat) * dateDays[n[3]]);
        }

        private static TimeSpan ToTimeSpanISO8601Time(String v)
        {
            IDictionary<String, Double> timeSeconds = new Dictionary<String, Double>()
            { {"H",3600D }, {"M",60D}, {"S",1D }, {"F", 0.1D } };

            v = v.Replace(",", ".");
            IList<String> n = v.RegexGroupsMatches(@"(\d+(\.\d+)?)(\D)");

            return TimeSpan.FromSeconds(Double.Parse(n[1], CultureInfo.InvariantCulture.NumberFormat) * timeSeconds[n[3]]);
        }

        public static DateTime ToDateTimeISO8601(this String v)
        {
            DateTime d;
            if (!v.TryToDateTimeISO8601(out d))
                throw new InvalidCastException("String is not ISO8601 valid date time.");

            return d;
        }

        public static bool TryToDateTimeISO8601(this String v, out DateTime ret)
        {
            ret = default(DateTime);
            try
            {
                v = v.Trim();

                bool utc = v.EndsWith("Z", StringComparison.CurrentCultureIgnoreCase);
                v.Replace("Z", "").Replace("z", "");

                ret = DateTime.ParseExact(v
                    , new string[] {
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd HH:mm:ss,fff",
                    "yyyy-MM-ddTHH:mm:ss",
                    "yyyy-MM-ddTHH:mm:ss,fff",
                    "yyyyMMddHHmmss",
                    "yyyyMMddHHmmssfff",
                    "yyyyMMddTHHmmss",
                    "yyyyMMddTHHmmssfff"
                        }
                    , CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None);

                return true;
            }
            catch {
                return false;
            }
        }

        public static DateTimeOffset ToDateTimeOffsetISO8601(this String v)
        {
            DateTimeOffset ret;

            if(!v.TryToDateTimeOffsetISO8601(out ret))
                throw new InvalidCastException("No valid datetime offset iso8601 cast '" + v + "'. Valid formats: yyyy[-]MM[-]dd[ T]HH[:]mm[:]ss[,fff][+-]HH[:]ss");

            return ret;
        }

        public static bool TryToDateTimeOffsetISO8601(this String v, out DateTimeOffset ret)
        {
            ret = default(DateTimeOffset);

            try
            {
                DateTime dateTime;
                TimeSpan offset = TimeSpan.Zero;
                Regex r = new Regex(@"(\d{4}-?\d{2}-?\d{2}T?\d{2}:?\d{2}:?\d{2}(,\d{3})?)(([+-]\d{2}(:?\d{2})?)|Z)?");
                Match m = r.Match(v);
                if (m.Success)
                {
                    dateTime = m.Groups[1].Value.ToDateTimeISO8601();
                    if (String.IsNullOrEmpty(m.Groups[3].Value))
                        offset = DateTimeOffset.Now.Offset;
                    else
                    {
                        if (m.Groups[3].Value == "Z")
                            offset = TimeSpan.Zero;
                        else
                        {
                            offset = TimeSpan.ParseExact(m.Groups[3].Value, new string[] { @"\+hh", @"\-hh", @"\+hh\:mm", @"\+hhmm", @"\-hh\:mm", @"\-hhmm" }, CultureInfo.InvariantCulture.DateTimeFormat);
                            if (m.Groups[3].Value[0] == '-')
                                offset = offset.Negate();
                        }
                    }
                    ret= new DateTimeOffset(dateTime, offset);

                    return true;
                }
                else
                    throw new InvalidCastException("No valid datetime offset iso8601 cast '" + v + "'. Valid formats: yyyy[-]MM[-]dd[ T]HH[:]mm[:]ss[,fff][+-]HH[:]ss");
            }
            catch {
                return false;
            }
        }
        
        public static DateTime GetEpochUnixTime(this DateTime v)
        {
            return new DateTime (1970, 1, 1);
        }

        public enum EpochPrecision { Seconds, Milliseconds}
        public static DateTime FromEpochUTCUnixTime(this DateTime v, long unixTime, EpochPrecision precision=EpochPrecision.Milliseconds)
        {
            if(precision== EpochPrecision.Seconds)
                return new DateTime(1970, 1, 1).AddSeconds(unixTime);
            else if (precision == EpochPrecision.Milliseconds)
                return new DateTime (1970, 1, 1).AddMilliseconds(unixTime);
            else
                return new DateTime(1970, 1, 1);
        }

        public static long ToEpochUTCUnixTime(this DateTime v, EpochPrecision precision = EpochPrecision.Milliseconds)
        {
            if (precision == EpochPrecision.Seconds)
                return (long)v.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
            else if (precision == EpochPrecision.Milliseconds)
                return (long)v.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
            else
                return 0;
        }
    }
}