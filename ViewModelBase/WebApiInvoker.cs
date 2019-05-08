using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;

namespace ViewModelBasic
{
    public class WebApiInvoker
    {
        public static WebApiInvoker Instance = new WebApiInvoker();

        HttpClient _client = null;

        public HttpClient Client { get { return _client; } }

        protected WebApiInvoker()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["WebApiSite"]);
        }

        public TResult Invoke<TResult, TParam>(TParam param, string apiUri)
        {
            MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
            HttpContent content = new ObjectContent<TParam>(param, jsonFormatter);

            var response = Client.PostAsync(apiUri, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsAsync<TResult>().Result;
            }
            else
            {
                throw new HttpRequestException("服务调用异常.(" + response.ReasonPhrase + ")");
            }
        }
    }
}
