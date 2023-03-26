using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DotNetAidLib.Core.Network.Client.Core
{
    public abstract class WebRequestFactory
    {
        public static HttpWebResponse Request(string method, Uri uri, string body, Encoding encoding,
            int timeout = 1000, IEnumerable<KeyValuePair<string, string>> headers = null,
            ICredentials credentials = null, IEnumerable<Cookie> cookies = null,
            X509CertificateCollection clientCertificates = null,
            RemoteCertificateValidationCallback hostCertificateValidationCallback = null)
        {
            return Request(method, uri, new MemoryStream(encoding.GetBytes(body)), timeout, headers, credentials,
                cookies, clientCertificates, hostCertificateValidationCallback);
        }

        public static HttpWebResponse Request(string method, Uri uri, Stream body = null, int timeout = 1000,
            IEnumerable<KeyValuePair<string, string>> headers = null, ICredentials credentials = null,
            IEnumerable<Cookie> cookies = null, X509CertificateCollection clientCertificates = null,
            RemoteCertificateValidationCallback hostCertificateValidationCallback = null)
        {
            try
            {
                Uri requestUri = null;
                NetworkCredential uriCredentials = null;

                uriCredentials = uri.GetCredentials(out requestUri);

                var wReq = (HttpWebRequest) WebRequest.Create(requestUri);
                wReq.Method = method;
                wReq.Timeout = timeout;

                if (body != null)
                    body.CopyTo(wReq.GetRequestStream());

                if (hostCertificateValidationCallback != null)
                    wReq.ServerCertificateValidationCallback = hostCertificateValidationCallback;

                if (clientCertificates != null)
                    wReq.ClientCertificates = clientCertificates;

                if (credentials != null)
                    wReq.Credentials = credentials;
                else
                    wReq.Credentials = uriCredentials;

                if (cookies != null)
                    cookies.ToList().ForEach(v => wReq.CookieContainer.Add(v));

                if (headers != null)
                    foreach (var header in headers)
                        if (wReq.Headers.AllKeys.Any(v => v.Equals(header.Key)))
                            wReq.Headers[header.Key] = header.Value;
                        else
                            wReq.Headers.Add(header.Key, header.Value);
                return (HttpWebResponse) wReq.GetResponse();
            }
            catch (Exception ex)
            {
                throw new WebClientException("Error getting web response to uri '" + uri + "'.", ex);
            }
        }

        public static Stream RequestStream(Uri uri, IEnumerable<KeyValuePair<string, string>> headers = null,
            int timeout = 1000)
        {
            Stream ret = null;

            try
            {
                var wReq = (HttpWebRequest) WebRequest.Create(uri);
                wReq.Timeout = timeout;
                wReq.UserAgent = "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:41.0) Gecko/20100101 Firefox/41.0";
                if (headers != null)
                    foreach (var header in headers)
                        wReq.Headers.Add(header.Key, header.Value);
                var wRes = wReq.GetResponse();
                ret = wRes.GetResponseStream();
            }
            catch (Exception ex)
            {
                throw new WebClientException("Error getting web response to uri '" + uri + "'.", ex);
            }

            return ret;
        }

        public static StreamReader RequestStreamReader(Uri uri, Encoding encoding = null,
            IEnumerable<KeyValuePair<string, string>> headers = null, int timeout = 1000)
        {
            if (encoding == null)
                return new StreamReader(RequestStream(uri, headers, timeout), Encoding.UTF8);
            return new StreamReader(RequestStream(uri, headers, timeout), encoding);
        }

        public static string RequestString(Uri uri, Encoding encoding = null,
            IEnumerable<KeyValuePair<string, string>> headers = null, int timeout = 1000)
        {
            StreamReader sr = null;
            try
            {
                sr = RequestStreamReader(uri, encoding, headers, timeout);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sr.Close();
            }
        }
    }
}