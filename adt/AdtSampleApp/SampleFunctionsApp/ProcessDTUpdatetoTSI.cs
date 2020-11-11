using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

namespace SampleFunctionsApp
{
    public static class ProcessDTUpdatetoTSI
    {
        [Disable]
        [FunctionName("ProcessDTUpdatetoTSI")]
        public static async Task Run(
            [EventHubTrigger("twinshub", Connection = "EventHubAppSetting-Twins")] EventData EventHubMessage,
            [EventHub("tsihub", Connection = "EventHubAppSetting-TSI")] IAsyncCollector<string> outputEvents,
            ILogger log)
        {
            log.LogInformation("Start execution");
            JObject message = (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(EventHubMessage.Body));
            log.LogInformation("[ProcessDTUpdatetoTSI] Reading event:" + message.ToString());

            // Read values that are replaced or added
            Dictionary<string, object> tsiUpdate = new Dictionary<string, object>();
            foreach (var operation in message["patch"])
            {
                if (operation["op"].ToString() == "replace" || operation["op"].ToString() == "add")
                {
                    //Convert from JSON patch path to a flattened property for TSI
                    //Example input: /Front/Temperature
                    //        output: Front.Temperature
                    string path = operation["path"].ToString().Substring(1);
                    path = path.Replace("/", ".");
                    tsiUpdate.Add(path, operation["value"]);
                }
            }
            //Send an update if updates exist
            if (tsiUpdate.Count > 0)
            {
                tsiUpdate.Add("$dtId", EventHubMessage.Properties["cloudEvents:subject"]);
                log.LogInformation("[ProcessDTUpdatetoTSI] TsiUpdate:" + JsonConvert.SerializeObject(tsiUpdate));
                await outputEvents.AddAsync(JsonConvert.SerializeObject(tsiUpdate));
            }
        }
    }
}