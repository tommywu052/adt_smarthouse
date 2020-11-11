using System;
using System.Net.Http;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.DigitalTwins.Core.Serialization;
using Azure.Identity;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SampleFunctionsApp
{
    // This class processes telemetry events from IoT Hub, reads temperature of a device
    // and sets the "Temperature" property of the device with the value of the telemetry.
    public class ProcessHubToDTEvents
    {
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("ProcessHubToDTEvents")]
        public async void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            // After this is deployed, you need to turn the Managed Identity Status to "On",
            // Grab Object Id of the function and assigned "Azure Digital Twins Owner (Preview)" role
            // to this function identity in order for this function to be authorized on ADT APIs.
            if (adtInstanceUrl == null) log.LogError("Application setting \"ADT_SERVICE_URL\" not set");
            log.LogInformation("[ProcessHubToDTEvents]");
            try
            {
                //Authenticate with Digital Twin
                ManagedIdentityCredential cred = new ManagedIdentityCredential("https://digitaltwins.azure.net");

                DigitalTwinsClient client = new DigitalTwinsClient(
                    new Uri(adtInstanceUrl), cred, new DigitalTwinsClientOptions
                    { Transport = new HttpClientTransport(httpClient) });
                // log.LogInformation($"ADT service client connection created.");

                if (eventGridEvent != null && eventGridEvent.Data != null)
                {
                    log.LogInformation(eventGridEvent.Data.ToString());

                    // Reading deviceId and temperature for IoT Hub JSON                    
                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];

                    switch (deviceId)
                    {
                        case "7a4c23e8cc86da8940b8d2203f75f8e62b67c04b67b981d6900ca67814f137929d4bb44bae637de91f8fe8bab720b5befc68ae043264ee4250e9592a86aba303":
                            deviceId = "Camera_LeftBottom_Floor_One";
                            break;
                        case "a5159553768b23b7062d12ff602065b5eddf4840a3c1ee1690f383310bd2d0bed94b5c6b21dc68fdd9a82f32ca8b1935484e6f25a3f381c94fff428c885dbc73":
                            deviceId = "Camera_Right_Floor_One";
                            break;
                    }

                    log.LogInformation("Event Grid Message:" + deviceMessage);
                    log.LogInformation("Device ID:" + deviceId);

                    //Update twin using device temperature
                    var uou = new UpdateOperationsUtility();

                    var smoke = deviceMessage["body"]["Smoke"];
                    if (smoke != null) {
                        log.LogInformation("Device Message - Smoke:" + smoke.ToString());
                        uou.AppendReplaceOp("/SmokeSensor/Smoke", smoke.Value<int>());
                    }

                    var distance = deviceMessage["body"]["Distance"];
                    if (distance != null)
                    {
                        log.LogInformation("Device Message - Distance:" + distance.ToString());
                        uou.AppendReplaceOp("/DistanceSensor/Distance", distance.Value<float>());
                    }

                    var flame = deviceMessage["body"]["Flame"];
                    if (flame != null)
                    {
                        log.LogInformation("Device Message - Flame:" + flame.ToString());
                        uou.AppendReplaceOp("/FlameSensor/Flame", flame.Value<int>());
                    }

                    var light = deviceMessage["body"]["Light"];
                    if (light != null)
                    {
                        log.LogInformation("Device Message - Light:" + light.ToString());
                        uou.AppendReplaceOp("/LightSensor/Light", light.Value<int>());
                    }

                    var pirState = deviceMessage["body"]["PIRState"];
                    if (pirState != null)
                    {
                        log.LogInformation("Device Message - PIRState:" + pirState.ToString());
                        uou.AppendReplaceOp("/PIRSensor/PIRState", pirState.Value<bool>());
                    }

                    var temperature = deviceMessage["body"]["Temperature"];
                    if (temperature != null)
                    {
                        log.LogInformation("Device Message - Temperature:" + temperature.ToString());
                        uou.AppendReplaceOp("/EnvironmentSensor/Temperature", temperature.Value<int>());
                    }

                    var humidity = deviceMessage["body"]["Humidity"];
                    if (humidity != null)
                    {
                        log.LogInformation("Device Message - Humidity:" + humidity.ToString());
                        uou.AppendReplaceOp("/EnvironmentSensor/Humidity", humidity.Value<int>());
                    }

                    var camera_label = deviceMessage["body"]["label"];
                    if (camera_label != null)
                    {
                        log.LogInformation("Device Message - Camera Label:" + camera_label.ToString());
                        uou.AppendReplaceOp("/label", camera_label.Value<string>());
                    }


                    //switch (deviceId)
                    //{
                    //    case "EnvironmentSensor":
                    //        var temperature = deviceMessage["body"]["Temperature"];
                    //        var humidity = deviceMessage["body"]["Humidity"];
                    //        log.LogInformation("Device Message:" + deviceMessage["body"].ToString());
                    //        uou.AppendReplaceOp("/Temperature", temperature.Value<int>());
                    //        uou.AppendReplaceOp("/Humidity", humidity.Value<int>());
                    //        break;
                    //    case "SmokeSensor":
                    //        var smoke = deviceMessage["body"]["Smoke"];
                    //        log.LogInformation("Device Message:" + deviceMessage["body"].ToString());
                    //        uou.AppendReplaceOp("/Smoke", smoke.Value<int>());
                    //        break;
                    //    case "FlameSensor":
                    //        var flame = deviceMessage["body"]["Flame"];
                    //        log.LogInformation("Device Message:" + deviceMessage["body"].ToString());
                    //        uou.AppendReplaceOp("/Flame", flame.Value<int>());
                    //        break;
                    //    case "PIRSensor":
                    //        var pirState = deviceMessage["body"]["PIRState"];
                    //        log.LogInformation("Device Message:" + deviceMessage["body"].ToString());
                    //        uou.AppendReplaceOp("/PIRState", pirState.Value<bool>());
                    //        break;
                    //    case "DistanceSensor":
                    //        var distance = deviceMessage["body"]["Distance"];
                    //        log.LogInformation("Device Message:" + deviceMessage["body"].ToString());
                    //        uou.AppendReplaceOp("/Distance", distance.Value<float>());
                    //        break;
                    //    default:
                    //        log.LogInformation("No match Device ID");
                    //        break;
                    //}

                    await client.UpdateDigitalTwinAsync(deviceId, uou.Serialize());



                    //switch (deviceId)
                    //{
                    //    case "Thermostat_1":
                    //        var temperature = deviceMessage["body"]["Temperature"];
                    //        log.LogInformation($"Device: {deviceId}, Temperature is: {temperature}");
                    //        //Update twin using device temperature
                    //        uou.AppendReplaceOp("/Temperature", temperature.Value<double>());
                    //        break;
                    //    case "Hygrometer_1":
                    //        var humidity = deviceMessage["body"]["Humidity"];
                    //        log.LogInformation($"Device: {deviceId}, Humidity is: {humidity}");
                    //        //Update twin using device temperature
                    //        uou.AppendReplaceOp("/Humidity", humidity.Value<double>());
                    //        break;
                    //    case "Booking_system_1":
                    //        var people_count = deviceMessage["body"]["People_count"];
                    //        log.LogInformation($"Device: {deviceId}, People_count is: {people_count}");
                    //        //Update twin using device temperature
                    //        uou.AppendReplaceOp("/People_count", people_count.Value<double>());
                    //        break;
                    //    case "Booking_system_2":
                    //        var people_count_2 = deviceMessage["body"]["People_count"];
                    //        log.LogInformation($"Device: {deviceId}, People_count is: {people_count_2}");
                    //        //Update twin using device temperature
                    //        uou.AppendReplaceOp("/People_count", people_count_2.Value<double>());
                    //        break;
                    //    case "Cutter_tool_1":
                    //        var spinspeed = deviceMessage["body"]["Spinspeed"];
                    //        log.LogInformation($"Device: {deviceId}, Spinspeed is: {spinspeed}");
                    //        //Update twin using device temperature
                    //        uou.AppendReplaceOp("/Spinspeed", spinspeed.Value<double>());
                    //        break;
                    //    case "Cutter_tool_2":
                    //        var spinspeed2 = deviceMessage["body"]["Spinspeed"];
                    //        log.LogInformation($"Device: {deviceId}, Spinspeed is: {spinspeed2}");
                    //        //Update twin using device temperature
                    //        uou.AppendReplaceOp("/Spinspeed", spinspeed2.Value<double>());
                    //        break;
                    //    case "Three_color_light_1":
                    //        var color = deviceMessage["body"]["Color"];
                    //        log.LogInformation($"Device: {deviceId}, Color is: {color}");
                    //        //Update twin using device temperature
                    //        uou.AppendReplaceOp("/Color", color.Value<string>());
                    //        break;
                    //    case "Three_color_light_2":
                    //        var color2 = deviceMessage["body"]["Color"];
                    //        log.LogInformation($"Device: {deviceId}, Color is: {color2}");
                    //        //Update twin using device temperature
                    //        uou.AppendReplaceOp("/Color", color2.Value<string>());
                    //        break;
                    //    default:
                    //        Console.WriteLine("Default case");
                    //        break;
                    //}
                    //await client.UpdateDigitalTwinAsync(deviceId, uou.Serialize());
                }
            }
            catch (Exception e)
            {
                log.LogError($"Error in ingest function: {e.Message}");
            }
        }
    }
}