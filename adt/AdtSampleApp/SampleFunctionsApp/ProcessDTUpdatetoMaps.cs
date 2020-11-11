using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace SampleFunctionsApp
{
    public static class ProcessDTUpdatetoMaps
    {   //Read maps credentials from application settings on function startup
        private static string statesetID = Environment.GetEnvironmentVariable("statesetID");
        private static string subscriptionKey = Environment.GetEnvironmentVariable("subscription-key");
        private static HttpClient httpClient = new HttpClient();

        [Disable]
        [FunctionName("ProcessDTUpdatetoMaps")]
        public static async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation("Start execution");
            // Self output
            // log.LogInformation("statesetID:" + statesetID);
            // log.LogInformation("subscriptionKey:" + subscriptionKey);
            string twinId = eventGridEvent.Subject.ToString();
            JObject message = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
            log.LogInformation("Reading event from twinID:" + twinId + ": " +
                eventGridEvent.EventType.ToString() + ": " + message["data"]);

            // "modelId": "dtmi:contosocom:DigitalTwins:Thermostat;1", "patch": [ { "value": 67.18639855840541, "path": "/Temperature", "op": "replace" } ]
            // log.LogInformation("Before if");
            // Parse updates to "space" twins
            if (message["data"]["modelId"].ToString() == "dtmi:example:Room;1")
            {
                // Set the ID of the room to be updated in your map. 
                // Replace this line with your logic for retrieving featureID. 
                // log.LogInformation("Into if");
                // string featureID = "UNIT64";
                string featureID = "";
                switch (twinId)
                {
                    case "Room_1":
                        featureID = "UNIT42";
                        break;
                    case "Room_2":
                        featureID = "UNIT31";
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }

                //Iterate through the properties that have changed
                foreach (var operation in message["data"]["patch"])
                {
                    if (operation["op"].ToString() == "replace" && operation["path"].ToString() == "/People_count")
                    {   //Update the maps feature stateset
                        var postcontent = new JObject(new JProperty("States", new JArray(
                            new JObject(new JProperty("keyName", "people"),
                                 new JProperty("value", operation["value"].ToString()),
                                 new JProperty("eventTimestamp", DateTime.Now.ToString("s"))))));

                        log.LogInformation("After Process Post Content");
                        //log.LogInformation(postcontent.ToString());

                        var response = await httpClient.PostAsync(
                            $"https://us.atlas.microsoft.com/featureState/state?api-version=1.0&statesetID={statesetID}&featureID={featureID}&subscription-key={subscriptionKey}",
                            new StringContent(postcontent.ToString()));

                        log.LogInformation(await response.Content.ReadAsStringAsync());
                    }
                }
            }
            else if (message["data"]["modelId"].ToString() == "dtmi:example:Three_color_light;1")
            {
                // Set the ID of the room to be updated in your map. 
                // Replace this line with your logic for retrieving featureID. 
                // log.LogInformation("Into if");
                // string featureID = "UNIT64";
                string featureID = "";
                switch (twinId)
                {
                    case "Three_color_light_1":
                        featureID = "UNIT84";
                        break;
                    case "Three_color_light_2":
                        featureID = "UNIT82";
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }

                //Iterate through the properties that have changed
                foreach (var operation in message["data"]["patch"])
                {
                    if (operation["op"].ToString() == "replace" && operation["path"].ToString() == "/Color")
                    {   //Update the maps feature stateset
                        var value = 0;
                        if (operation["value"].ToString() == "green") value = 2;
                        else if (operation["value"].ToString() == "yellow") value = 1;
                        else if (operation["value"].ToString() == "red") value = 0;
                        var postcontent = new JObject(new JProperty("States", new JArray(
                            new JObject(new JProperty("keyName", "color"),
                                 new JProperty("value", value.ToString()),
                                 new JProperty("eventTimestamp", DateTime.Now.ToString("s"))))));

                        log.LogInformation("After Process Post Content");
                        //log.LogInformation(postcontent.ToString());

                        var response = await httpClient.PostAsync(
                            $"https://us.atlas.microsoft.com/featureState/state?api-version=1.0&statesetID={statesetID}&featureID={featureID}&subscription-key={subscriptionKey}",
                            new StringContent(postcontent.ToString()));

                        log.LogInformation(await response.Content.ReadAsStringAsync());
                    }
                }
            }

        }
    }
}