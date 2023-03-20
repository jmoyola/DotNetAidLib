using System;
namespace DotNetAidLib.Core.Network.Client
{
    public abstract class WebRequestHeaders
    {
        public static readonly String AIM = "A-IM";
        public static readonly String Accept = "Accept";
        public static readonly String AcceptCharset = "Accept-Charset";
        public static readonly String AcceptEncoding = "Accept-Encoding";
        public static readonly String AcceptLanguage = "Accept-Language";
        public static readonly String AcceptDatetime = "Accept-Datetime";
        public static readonly String AccessControlRequestMethod = "Access-Control-Request-Method";
        public static readonly String Authorization = "Authorization";
        public static readonly String CacheControl = "Cache-Control";
        public static readonly String Connection = "Connection";
        public static readonly String ContentLength = "Content-Length";
        public static readonly String ContentMD5 = "Content-MD5";
        public static readonly String ContentType = "Content-Type";
        public static readonly String Cookie = "Cookie";
        public static readonly String Date = "Date";
        public static readonly String Spect = "Spect";
        public static readonly String Forwarded = "Forwarded";
        public static readonly String From = "From";
        public static readonly String Host = "Host";

        public static readonly String IfMatch = "If-Match";
        public static readonly String IfModifiedSince = "If-Modified-Since";
        public static readonly String IfNoneMatch = "If-None-Match";
        public static readonly String IfRange = "If-Range";
        public static readonly String IfUnmodifiedSince = "If-Unmodified-Since";
        public static readonly String MaxForwards = "Max-Forwards";
        public static readonly String Origin = "Origin";
        public static readonly String Pragma = "Pragma";
        public static readonly String Range = "Range";
        public static readonly String Referer = "Referer";
        public static readonly String TransferEncoding = "TE";
        public static readonly String UserAgent = "User-Agent";
        public static readonly String Upgrade = "Upgrade";
    }
}
