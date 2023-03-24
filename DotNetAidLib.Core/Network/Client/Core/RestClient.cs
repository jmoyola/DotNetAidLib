using System;
using System.Web;
using System.IO;
using System.Net;
using System.Collections.Generic;
using DotNetAidLib.Core.Serializer;

namespace DotNetAidLib.Core.Network.Client.Core
{
    public class RestClient
    {

        private WebProxy _Proxy;

        private ISerializer _Serializer = new JsonDCSerializer();

        private int _Timeout = 60000;

        public RestClient()
        {
        }

        public RestClient(ISerializer serializer)
        {
            this.Serializer = serializer;
        }

        public RestClient(WebProxy proxy)
        {
            this.Proxy = proxy;
        }

        public ISerializer Serializer
        {
            get
            {
                return _Serializer;
            }
            set
            {
                if ((value == null))
                {
                    throw new Exception("Serializer can\'t be null.");
                }

                _Serializer = value;
            }
        }

        public WebProxy Proxy
        {
            get
            {
                return _Proxy;
            }
            set
            {
                _Proxy = value;
            }
        }

        public int Timeout
        {
            get
            {
                return _Timeout;
            }
            set
            {
                if ((value < 1000))
                {
                    throw new TimeoutException("Timeout must be >= 1000");
                }

                _Timeout = value;
            }
        }

        public T GetMethod<T>(string url, Dictionary<string, object> parameters)
        {
            return _Serializer.Deserialize<T>(this.GetMethodRaw(url, parameters));
        }

        public Stream GetMethodRaw(string url, Dictionary<string, object> parameters)
        {
            try
            {
                //  Procesamos los parametros
                string urlParams = this.ParamsToUrlString(parameters);
                //  Esp�cificamos la url en funcion si hab�a ya par�metros o no
                string urlReq = null;
                if ((url.IndexOf("?") > -1))
                {
                    urlReq = (url + ("&" + urlParams));
                }
                else
                {
                    urlReq = (url + ("?" + urlParams));
                }

                HttpWebRequest req = ((HttpWebRequest)(HttpWebRequest.Create(urlReq)));
                req.Method = "GET";
                req.ProtocolVersion = System.Net.HttpVersion.Version11;
                req.Timeout = _Timeout;
                req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                req.UserAgent = "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:41.0) Gecko/20100101 Firefox/41.0";
                req.Headers.Add("es-ES,es;q=0.8,en-US;q=0.5,en;q=0.3");
                // req.Headers.Add("Accept-Encoding: gzip, deflate")
                req.Headers.Add("DNT: 1");
                req.Headers.Add("Cookie: lastSeen=0");
                req.ReadWriteTimeout = _Timeout;
                req.AllowAutoRedirect = true;
                return req.GetResponse().GetResponseStream();
            }
            catch (Exception ex)
            {
                throw new Exception(("Error al realizar la petici�n get a la url \'"
                                + (url + ("\'.\\r\\n" + ex.ToString()))));
            }

        }

        private string ParamsToUrlString(Dictionary<string, object> parameters)
        {
            //  Procesamos los parametros
            string urlParams = null;
            if (!(parameters == null)) {
                foreach (KeyValuePair<string, object> kvp in parameters) {
                    if ((kvp.Value == null))
                    {
                        urlParams = (urlParams + ("&"
                                    + (HttpUtility.UrlEncode(kvp.Key) + "=")));
                    }
                    else if (!kvp.Value.GetType().IsArray)
                    {
                        urlParams = (urlParams + ("&"
                                    + (HttpUtility.UrlEncode(kvp.Key) + ("=" + HttpUtility.UrlEncode(kvp.Value.ToString())))));
                    }
                    else
                    {
                        Array arr = ((Array)(kvp.Value));
                        for (long ia = 0; (ia
                                    <= (arr.GetLongLength(1) - 1)); ia++)
                        {
                            object arrVal = arr.GetValue(ia);
                            if ((arrVal == null))
                            {
                                urlParams = (urlParams + ("&"
                                            + (HttpUtility.UrlEncode(kvp.Key) + "=")));
                            }
                            else
                            {
                                urlParams = (urlParams + ("&"
                                            + (HttpUtility.UrlEncode(kvp.Key) + ("=" + HttpUtility.UrlEncode(arrVal.ToString())))));
                            }

                        }

                    }

                }

                if (!string.IsNullOrEmpty(urlParams))
                {
                    urlParams = urlParams.Substring(1);
                }

            }

            return urlParams;
        }
    }
}
