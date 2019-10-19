using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
    {
        class StringTable
        {
            public string[] ColumnNames { get; set; }
            public string[,] Values { get; set; }
        }

        public class AzureMLService
        {
            public string result = "No result";

            public static AzureMLService Instance = new AzureMLService();

            private AzureMLService() { }

            private void JSONParser(string result)
            {
                // JSON parsing
                dynamic responseAzure = JObject.Parse(result);
                dynamic output1 = responseAzure.Results;
                dynamic value = output1.output1;
                dynamic values = value.value;
                dynamic obj = null;
                foreach (var el in values.Values.Children())
                    obj = el;

                foreach (var el in obj)
                    this.result = el.ToString();
        }

            public async Task<bool> InvokeRequestForSmartphonesData(string[] input)
            {
                result = "No result";

                using (var client = new HttpClient())
                {
                    var scoreRequest = new
                    {

                        Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"Camera", "Memory", "GPU", "Processor", "Screen", "Battery", "Price", "Innovation", "Smartphones"},
                                Values = new string[,] {  { input[0], input[1], input[2], input[3], input[4], input[5], input[6], input[7], "value" }, { input[0], input[1], input[2], input[3], input[4], input[5], input[6], input[7], "value" },  }
                            }
                        },
                    },
                        GlobalParameters = new Dictionary<string, string>()
                        {
                        }
                    };
                    const string apiKey = "5MuWuxM62vgWRn7+dPF1vtFcFphD1jh+thwIurdE4r9Rv2a8QCI4EbdVU53JwtviJnsJbtZsstgW1OY+kpE8yQ=="; // Replace this with the API key for the web service
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/29070115c5b64b3fb7bc5c5d92a02edb/services/4df7d29ec7db470eb929909fe2d8e967/execute?api-version=2.0&details=true");

                    HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        JSONParser(result);
                        

                }

                }
            return true;
            }

            public async Task<bool> InvokeRequestForLaptopsData(string[] input)
            {
                using (var client = new HttpClient())
                {
                    var scoreRequest = new
                    {

                        Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"ProcessorCores", "ProcessorType", "GPU", "RAM", "Memory", "CardReader", "Ports", "HDMI", "CDReader", "Type", "Price", "OS", "Size", "GPU_Type", "Laptop", "EMAG", "CEL", "FLANCO"},
                                Values = new string[,] {  { input[0], input[1], input[2], input[3], input[4], input[5], input[6], input[7], input[8], input[9], input[10], input[11], input[12], input[13], "value", "value", "value", "value" },  { input[0], input[1], input[2], input[3], input[4], input[5], input[6], input[7], input[8], input[9], input[10], input[11], input[12], input[13], "value", "value", "value", "value" },  }
                            }
                        },
                    },
                        GlobalParameters = new Dictionary<string, string>()
                        {
                        }
                    };
                    const string apiKey = "SvJ3yeDPAe2rW6zg5ZmOgEsofjS3l7oRwNEeTVA1wtvkTjlxNWiUzdXRD1gvCXlnF9OGrzf36jjcIUpd0w+qLA=="; // Replace this with the API key for the web service
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/29070115c5b64b3fb7bc5c5d92a02edb/services/8e132d56f4af4f09936f5fb36ade8199/execute?api-version=2.0&details=true");

                    HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                    if (response.IsSuccessStatusCode)
                    {
                    string result = await response.Content.ReadAsStringAsync();
                        JSONParser(result);
                    }

                }

            return true;
            }
        }
    }


