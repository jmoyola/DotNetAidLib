namespace DotNetAidLib.Core.Network.Client.Core
{
    public abstract class WebRequestHeaders
    {
        public static readonly string AIM = "A-IM";
        public static readonly string Accept = "Accept";
        public static readonly string AcceptCharset = "Accept-Charset";
        public static readonly string AcceptEncoding = "Accept-Encoding";
        public static readonly string AcceptLanguage = "Accept-Language";
        public static readonly string AcceptDatetime = "Accept-Datetime";
        public static readonly string AccessControlRequestMethod = "Access-Control-Request-Method";
        public static readonly string Authorization = "Authorization";
        public static readonly string CacheControl = "Cache-Control";
        public static readonly string Connection = "Connection";
        public static readonly string ContentLength = "Content-Length";
        public static readonly string ContentMD5 = "Content-MD5";
        public static readonly string ContentType = "Content-Type";
        public static readonly string Cookie = "Cookie";
        public static readonly string Date = "Date";
        public static readonly string Spect = "Spect";
        public static readonly string Forwarded = "Forwarded";
        public static readonly string From = "From";
        public static readonly string Host = "Host";

        public static readonly string IfMatch = "If-Match";
        public static readonly string IfModifiedSince = "If-Modified-Since";
        public static readonly string IfNoneMatch = "If-None-Match";
        public static readonly string IfRange = "If-Range";
        public static readonly string IfUnmodifiedSince = "If-Unmodified-Since";
        public static readonly string MaxForwards = "Max-Forwards";
        public static readonly string Origin = "Origin";
        public static readonly string Pragma = "Pragma";
        public static readonly string Range = "Range";
        public static readonly string Referer = "Referer";
        public static readonly string TransferEncoding = "TE";
        public static readonly string UserAgent = "User-Agent";
        public static readonly string Upgrade = "Upgrade";
    }
}