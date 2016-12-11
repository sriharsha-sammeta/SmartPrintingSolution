using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPrinter
{
    class Program
    {
        private static string aadInstance = "https://login.windows.net/{0}";
        private static string tenant = ConfigurationManager.AppSettings["Tenant"];
        private static string clientId = ConfigurationManager.AppSettings["ClientID"];
        private static string clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        private static string authority = String.Format(aadInstance, tenant);
        private static string apiResourceId = ConfigurationManager.AppSettings["ApiResourceID"];

        async static Task<HttpResponseMessage> SmartPrinterServiceCall(string queryString, HttpClient client, string content)
        {            
            var jsonString = JsonConvert.SerializeObject(content);
            var fileContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(queryString, fileContent);
            return response;
        }
        static async Task<AuthenticationResult> GetToken()
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(clientId, clientSecret);
            var token = await authContext.AcquireTokenAsync(apiResourceId, clientCred);
            return token;
        }
        static void Main(string[] args)
        {
            try
            {                
                Stream inputStream = Console.OpenStandardInput();
                StreamReader reader = new StreamReader(inputStream);                
                string content = reader.ReadToEnd();
                int copiesCountOffset = 10;
                string userAlias = args[0];
                string fileName = args[1];

                int copies = content.IndexOf("NumCopies") + copiesCountOffset;
                int copiesEnd = content.IndexOf("\r", copies);
                int numberOfCopies;
                if (copies == 9)
                {
                    numberOfCopies = 1;
                }
                else
                {
                    string rawNumber = content.Substring(copies, copiesEnd - copies);
                    numberOfCopies = int.Parse(rawNumber);
                }
                int fileOffset = 7;
                int fileIndex = content.IndexOf("Title:") + fileOffset;
                int fileEnd = content.IndexOf("\r", fileIndex);
                fileName = content.Substring(fileIndex, fileEnd - fileIndex);
                if (content.Contains("%%BeginDefaults") && content.Contains("%%BeginProlog"))
                {
                    //WebRequestHandler handler = new WebRequestHandler();
                    //X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    //store.Open(OpenFlags.ReadOnly);
                    //X509Certificate2 authCert = new X509Certificate2();
                    //foreach (var cert in store.Certificates)
                    //{
                    //    if (cert.SubjectName.Name == "localhost")
                    //    {
                    //        authCert = cert;
                    //    }
                    //}
                    //handler.ClientCertificates.Add(authCert);
                    //store.Close();
                    var token = GetToken();
                    using (var client = new HttpClient())
                    {
                        string serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
                        client.BaseAddress = new Uri(serverAddress);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Result.AccessToken);
                        string queryString = "api/printjob/create/" + userAlias + "/" + fileName + ".ps/" + numberOfCopies;
                        HttpResponseMessage response = SmartPrinterServiceCall(queryString, client, content).Result;
                    }
                }

            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Virtual Printer";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);

            }
        }
    }
}
