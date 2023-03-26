using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using DotNetAidLib.Core.Serializer;

namespace DotNetAidLib.Core.Network.Client.Core
{
    public class RestClient
    {
        private ISerializer _Serializer = new JsonDCSerializer();

        private int _Timeout = 60000;

        public RestClient()
        {
        }

        public RestClient(ISerializer serializer)
        {
            Serializer = serializer;
        }

        public RestClient(WebProxy proxy)
        {
            Proxy = proxy;
        }

        public ISerializer Serializer
        {
            get => _Serializer;
            set
            {
                if (value == null) throw new Exception("Serializer can\'t be null.");

                _Serializer = value;
            }
        }

        public WebProxy Proxy { get; set; }

        public int Timeout
        {
            get => _Timeout;
            set
            {
                if (value < 1000) throw new TimeoutException("Timeout must be >= 1000");

                _Timeout = value;
            }
        }

        public T GetMethod<T>(string url, Dictionary<string, object> parameters)
        {
            return _Serializer.Deserialize<T>(GetMethodRaw(url, parameters));
        }

        public Stream GetMethodRaw(string url, Dictionary<string, object> parameters)
        {
            try
            {
                //  Procesamos los parametros
                var urlParams = ParamsToUrlString(parameters);
                //  Esp�cificamos la url en funcion si hab�a ya par�metros o no
                string urlReq = null;
                if (url.IndexOf("?") > -1)
                    urlReq = url + "&" + urlParams;
                else
                    urlReq = url + "?" + urlParams;

                var req = (HttpWebRequest) WebRequest.Create(urlReq);
                req.Method = "GET";
                req.ProtocolVersion = HttpVersion.Version11;
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
                throw new Exception("Error al realizar la petici�n get a la url \'"
                                    + url + ("\'.\\r\\n" + ex));
            }
        }

        private string ParamsToUrlString(Dictionary<string, object> parameters)
        {
            //  Procesamos los parametros
            string urlParams = null;
            if (!(parameters == null))
            {
                foreach (var kvp in parameters)
                    if (kvp.Value == null)
                    {
                        urlParams = urlParams + "&"
                                              + HttpUtility.UrlEncode(kvp.Key) + "=";
                    }
                    else if (!kvp.Value.GetType().IsArray)
                    {
                        urlParams = urlParams + "&"
                                              + HttpUtility.UrlEncode(kvp.Key) + "=" +
                                              HttpUtility.UrlEncode(kvp.Value.ToString());
                    }
                    else
                    {
                        var arr = (Array) kvp.Value;
                        for (long ia = 0;
                             ia
                             <= arr.GetLongLength(1) - 1;
                             ia++)
                        {
                            var arrVal = arr.GetValue(ia);
                            if (arrVal == null)
                                urlParams = urlParams + "&"
                                                      + HttpUtility.UrlEncode(kvp.Key) + "=";
                            else
                                urlParams = urlParams + "&"
                                                      + HttpUtility.UrlEncode(kvp.Key) + "=" +
                                                      HttpUtility.UrlEncode(arrVal.ToString());
                        }
                    }

                if (!string.IsNullOrEmpty(urlParams)) urlParams = urlParams.Substring(1);
            }

            return urlParams;
        }
    }
}