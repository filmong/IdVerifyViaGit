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

        [FunctionName("PerformIdVerify")]
        public static async Task<IActionResult> Run1Async(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req,
            TraceWriter log)
        {
            log.Info("HTTP triggered request is started processing...");            

            log.Info("HTTP request is completed processing.");
            return new OkObjectResult("success");
        }

        [FunctionName("getIdVerify")]
        public static async Task<IActionResult> Run2Async2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequest req,
            TraceWriter log)
        {
            log.Info("HTTP triggered, request processing started...");       

            log.Info("Http request is processed successfully.");

            // return to client appropriate object result saying request is processed
            return new OkObjectResult("success");
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
