namespace IdVerify
{
    using System;
    using System.IO;
    using System.Net.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Extensions.Primitives;
    using System.Threading.Tasks;
    using System.Net;

    /// <summary>
    /// IdVerify Class
    /// </summary>
    public static class IdVerify
    {
        const string AcceptHeader = "Accept";
        const string AcceptHeaderApplicationJson = "application/json";
        const string UserAgentHeader = "User-Agent";
        const string UserAgentValue = "MGM Resorts";
        const string AuthorizationHeader = "Authorization";
        const string AuthorizationHeaderValue = "";

        [FunctionName("doIdVerify")]
        public static async Task<IActionResult> Run1Async(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req,
            TraceWriter log)
        {
            log.Info("HTTP triggered request is started processing...");

            // client input
            string country = ExtractValue(req, "country");
            string idtype = ExtractValue(req, "idtype");

            var formCollection = req.Form;

            // client input, read pic file.
            string base64FrontImageString = string.Empty;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (req.HasFormContentType)
                {
                    var files = formCollection.Files;
                    // files["frontsideImage"].CopyTo(memoryStream);

                    files.GetFile("frontsideImage").CopyTo(memoryStream);
                    base64FrontImageString = Convert.ToBase64String(memoryStream.ToArray());
                }
            }

            // build httpRequest to Vendor API
            HttpRequestMessage requestMessage = new HttpRequestMessage();
            var headers = requestMessage.Headers;
            headers.Add(AcceptHeader, AcceptHeaderApplicationJson);
            headers.Add(UserAgentHeader, UserAgentValue);
            headers.Add(AuthorizationHeader, AuthorizationHeaderValue);
            requestMessage.Method = HttpMethod.Post;
            string performVerifyURLConfigElement = GetAppConfigValue(log, "PerformNetVerifyURI");
            requestMessage.RequestUri = new Uri(performVerifyURLConfigElement);
            requestMessage.Version = HttpVersion.Version11;

            // body on POST to Id Verify, create the whole request content.
            // merchantIdScanReference is the id the service uses for a request to submit to NetVerify.
            Guid merchantIdScanReference = Guid.NewGuid();

            string content = $"{{\"merchantIdScanReference\": \"{merchantIdScanReference}\", \"frontsideImage\": \"{base64FrontImageString}\", \"enabledFields\":\"idFirstName,idLastName\", \"country\": \"{country}\", \"idType\": \"{idtype}\"}}";
            StringContent bodyContent = new StringContent(content, System.Text.Encoding.UTF32, "application/json");

            requestMessage.Content = bodyContent;

            // send the request
            HttpClient httpClient = new HttpClient();
            log.Info($"Request send to requestMessage.RequestUri is {requestMessage.RequestUri}");
            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(requestMessage);
            var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
            log.Info($"Response from requestMessage.RequestUri is {responseString}");

            // TODO: process result

            // TODO: update patron for member id is verified


            log.Info("HTTP request is completed processing.");
            return httpResponseMessage.IsSuccessStatusCode
                ? (ActionResult)new OkObjectResult(responseString)
                : new BadRequestObjectResult(responseString);
        }

        [FunctionName("getIdVerify")]
        public static async Task<IActionResult> Run2Async2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequest req,
            TraceWriter log)
        {
            log.Info("HTTP triggered, request processing started...");

            // client input
            string scanIdReference = req.Query["scanId"];
            log.Info($"scanIdReference {scanIdReference}");

            // build httpRequest to Vendor API
            HttpRequestMessage requestMessage = new HttpRequestMessage();
            var headers = requestMessage.Headers;
            headers.Add(AcceptHeader, AcceptHeaderApplicationJson);
            headers.Add(UserAgentHeader, UserAgentValue);
            headers.Add(AuthorizationHeader, AuthorizationHeaderValue);
            requestMessage.Method = HttpMethod.Get;
            string getNetVerifyURL = GetAppConfigValueAndFormat(log, "GetNetVerifyURI", scanIdReference);

            requestMessage.RequestUri = new Uri(getNetVerifyURL);
            requestMessage.Version = HttpVersion.Version11;

            // send the request
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            HttpResponseMessage httpResponseMessage = response;

            // TODO: process result

            // TODO: update patron for member id is verified

            log.Info("Http request is processed successfully.");

            // return to client appropriate object result saying request is processed
            return httpResponseMessage.IsSuccessStatusCode
                ? (ActionResult)new OkObjectResult(await httpResponseMessage.Content.ReadAsStringAsync())
                : new BadRequestObjectResult(await httpResponseMessage.Content.ReadAsStringAsync());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetAppConfigValue(TraceWriter log, string key)
        {
            var value = GetEnvironmentVariable(key);
            log.Info($"Config key {key} and retrieved value is {value}");
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="key"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        // sample config setting is "GetNetVerifyURI": "https://netverify.com/api/netverify/v2/scans/{0}/data"
        private static string GetAppConfigValueAndFormat(TraceWriter log, string key, string parameterValue)
        {
            var value = GetAppConfigValue(log, key);
            var formattedValue = string.Format(value, parameterValue);
            return formattedValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string ExtractValue(HttpRequest req, string key)
        {
            StringValues idtypeFromHeaderValues = req.Headers[key];
            string value;
            if (!string.IsNullOrEmpty(idtypeFromHeaderValues))
            {
                value = idtypeFromHeaderValues;
            }
            else
            {
                value = req.Query[key];
            }

            return value;
        }
    }
}
