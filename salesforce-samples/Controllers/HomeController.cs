using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using salesforce_samples.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace salesforce_samples.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration Configuration;

        private string authToken;
        private string instanceUrl;


        private string loginEndpoint;
        private string servicesUrl;

        public HomeController(IConfiguration configuration)
        {
            Configuration = configuration;
            loginEndpoint = Configuration["SalesforceLoginEndpoint"];
            servicesUrl = Configuration["SalesforceServicesPath"];
        }        

        public IActionResult Index()
        {
            ViewBag.Endpoint = loginEndpoint;
            ViewBag.ServicesPath = servicesUrl;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<string> UploadAttachment(string endpoint, string servicesPath)
        {
            var formData = new MultipartFormDataContent("--------------------------741370981761916076903434");
                
            FileStream fs = new FileStream("example.html", FileMode.Open, FileAccess.Read);
            var streamContent = new StreamContent(fs);
            streamContent.Headers.Add("Content-Type", "application/octet-stream");

            formData.Add(new StringContent("{\"ParentId\":\"0033O00000d0x2CQAQ\", \"Name\":\"somethingElse\"}", System.Text.Encoding.UTF8, "application/json"), "ParentId");
            formData.Add(streamContent, "Body", "fileName");

            return await CallSalesforceApi(HttpMethod.Post, "sobjects/Attachment", formData);
        }

        public async Task<string> GetObjects(string endpoint, string servicesPath)
        {
            loginEndpoint = endpoint;
            servicesUrl = servicesPath;
            return await CallSalesforceApi(HttpMethod.Get, "tooling/sobjects/");
        }

        public async Task<string> GetDescriptions(string endpoint, string servicesPath)
        {            
            return await RunQuery(endpoint, servicesPath, "select DeveloperName, QualifiedApiName from EntityDefinition");
        }

        public async Task<string> DeleteAttachment(string endpoint, string servicesPath, string id)
        {
            loginEndpoint = endpoint;
            servicesUrl = servicesPath;
            return await CallSalesforceApi(HttpMethod.Delete, $"sobjects/Attachment/{id}");
        }

        public async Task<string> GetAttachments(string endpoint, string servicesPath, string name)
        {            
            var ret = string.Empty;

            loginEndpoint = endpoint;
            servicesUrl = servicesPath;

            var query = $"select Id from Attachment Where Name = '{name}'";

            var response = await CallSalesforceApi(HttpMethod.Get, query);

            var ids = new List<string>();

            try
            {
                var records = JsonSerializer.Deserialize<SalesforceQueryResponse<SalesforceQueryAttachment>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ids = records.Records.Select(e => e.Id).ToList();
            }
            catch(Exception e)
            {
                ret = e.Message;
            }                        

            ret = string.Join("\n", ids);

            return ret;
        }


        public async Task<string> RunQuery(string endpoint, string servicesPath, string query)
        {
            loginEndpoint = endpoint;
            servicesUrl = servicesPath;

            return await CallSalesforceApi(HttpMethod.Get, $"query?q={query}");
        }

        public async Task<string> CallSalesforceApi(HttpMethod httpMethod, string param, MultipartFormDataContent formData = null)
        {
            string ret = "";

            await Authorize();

            using (var _client = new HttpClient())
            {
                var request = new HttpRequestMessage(httpMethod, $"{instanceUrl}{servicesUrl}/{param}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                
                if (formData != null)
                    request.Content = formData;
                
                try
                {
                    var response = await _client.SendAsync(request);
                    ret = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    ret = e.Message;
                }
            }

            return ret;
        }

        public async Task Authorize()
        {
            using (var _client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                    {"grant_type","password"},
                    {"client_id", Configuration["SalesforceClientId"] },            
                    {"client_secret", Configuration["SalesforceClientSecret"] },
                    {"username", Configuration["SalesforceUserName"]},
                    {"password", Configuration["SalesforcePassword"]}
                });

                var response = await _client.PostAsync(loginEndpoint, content);
                var message = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(message);

                authToken = (string)obj["access_token"];
                instanceUrl = (string)obj["instance_url"];                
            }
        }
        
        public class SalesforceQueryResponse<T> where T : ISalesforceQueryRecord
        {
            public int TotalSize { get; set; }
            public bool Done { get; set; }
            public List<T> Records { get; set; }
        }

        public class SalesforceQueryAttributes
        {
            public string Type { get; set; }
            public string Url { get; set; }
        }

        public interface ISalesforceQueryRecord
        {
            public SalesforceQueryAttributes Attributes { get; set; }
        }

        public class SalesforceQueryAttachment : ISalesforceQueryRecord
        {
            public SalesforceQueryAttributes Attributes { get; set; }
            public string Id { get; set; }
        }
    }
}