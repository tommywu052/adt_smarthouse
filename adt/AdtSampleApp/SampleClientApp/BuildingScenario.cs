using System.Threading.Tasks;

namespace SampleClientApp
{
    public class BuildingScenario
    {
        private readonly CommandLoop cl;
        public BuildingScenario(CommandLoop cl)
        {
            this.cl = cl;
        }

        public async Task InitBuilding()
        {
            Log.Alert($"Deleting all twins...");
            await cl.DeleteAllTwinsAsync();
            Log.Out($"Creating 1 floor, 2 rooms, 1 thermostat, 1 hygrometer, 1 Booking system, 1 plant, 2 CNC machines, 1 three color light and 1 cutter tool");
            await InitializeGraph();
        }

        private async Task InitializeGraph()
        {
            string[] modelsToUpload = new string[10] {"CreateModels", "Booking_system", "CNC_Machine", "Cutter_tool", "Floor", "Hygrometer", "Plant", "Room", "Thermostat", "Three_color_light"};
            Log.Out($"Uploading {string.Join(", ", modelsToUpload)} models");

            await cl.CommandCreateModels(modelsToUpload);

            Log.Out($"Creating Models. ");
            await cl.CommandCreateDigitalTwin(new string[6]
                {
                    "CreateTwin", "dtmi:example:Floor;1", "Floor_1",
                    "Location", "string", "Floor_1",
                });
            await cl.CommandCreateDigitalTwin(new string[21]
                {
                    "CreateTwin", "dtmi:example:Room;1", "Room_1",
                    "Location", "string", "Floor_1",
                    "Temperature", "double", "0",
                    "Humidity", "double", "0",
                    "RoomName", "string", "Room_1",
                    "People_count", "integer", "0",
                    "Status", "string", "available",
                });
            await cl.CommandCreateDigitalTwin(new string[21]
                {
                    "CreateTwin", "dtmi:example:Room;1", "Room_2",
                    "Location", "string", "Floor_1",
                    "Temperature", "double", "0",
                    "Humidity", "double", "0",
                    "RoomName", "string", "Room_2",
                    "People_count", "integer", "0",
                    "Status", "string", "available",
                });
            await cl.CommandCreateDigitalTwin(new string[12]
                {
                    "CreateTwin", "dtmi:example:Thermostat;1", "Thermostat_1",
                    "Location", "string", "Room_1",
                    "Temperature", "double", "0",
                    "FirmwareVersion", "string", "0.0.1"
                });

            await cl.CommandCreateDigitalTwin(new string[12]
                {
                    "CreateTwin", "dtmi:example:Hygrometer;1", "Hygrometer_1",
                    "Location", "string", "Room_1",
                    "Humidity", "double", "0",
                    "deviceStatus", "string", "normal"
                });

            await cl.CommandCreateDigitalTwin(new string[15]
                {
                    "CreateTwin", "dtmi:example:Booking_system;1", "Booking_system_1",
                    "Location", "string", "Room_1",
                    "People_count", "integer", "0",
                    "People_limit", "integer", "20",
                    "Status", "string", "available"
                });
            await cl.CommandCreateDigitalTwin(new string[6]
                {
                    "CreateTwin", "dtmi:example:Plant;1", "Plant_1",
                    "Location", "string", "Plant_1"
                });
            await cl.CommandCreateDigitalTwin(new string[15]
                {
                    "CreateTwin", "dtmi:example:CNC_machine;1", "CNC_machine_1",
                    "Location", "string", "Plant_1",
                    "Spinspeed", "double", "0",
                    "Current_usage", "integer", "0",
                    "Color", "string", "red"
                });
            await cl.CommandCreateDigitalTwin(new string[15]
                {
                    "CreateTwin", "dtmi:example:CNC_machine;1", "CNC_machine_2",
                    "Location", "string", "Plant_1",
                    "Spinspeed", "double", "0",
                    "Current_usage", "integer", "0",
                    "Color", "string", "red"
                });
            await cl.CommandCreateDigitalTwin(new string[9]
                {
                    "CreateTwin", "dtmi:example:Three_color_light;1", "Three_color_light_1",
                    "Color", "string", "red",
                    "Location", "string", "CNC_machine_1"
                });
            await cl.CommandCreateDigitalTwin(new string[15]
                {
                    "CreateTwin", "dtmi:example:Cutter_tool;1", "Cutter_tool_1",
                    "Cutter_model", "string", "Undefined",
                    "Location", "string", "Plant_1",
                    "Spinspeed", "double", "0",
                    "Maximum_usage", "integer", "1000"
                });
            Log.Out($"Creating Edges. ");
            // Floor to Room
            await cl.CommandCreateRelationship(new string[5]
                {
                    "CreateEdge", "Floor_1", "contains", "Room_1", "Floor_1_to_Room_1"
                });
            await cl.CommandCreateRelationship(new string[5]
                {
                    "CreateEdge", "Floor_1", "contains", "Room_2", "Floor_1_to_Room_2"
                });
            await cl.CommandCreateRelationship(new string[5]
                {
                    "CreateEdge", "Room_1", "contains", "Thermostat_1", "Room_1_to_Thermostat_1"
                });
            await cl.CommandCreateRelationship(new string[5]
                {
                    "CreateEdge", "Room_1", "contains", "Hygrometer_1", "Room_1_to_Hygrometer_1"
                });
            await cl.CommandCreateRelationship(new string[5]
                {
                    "CreateEdge", "Room_1", "contains", "Booking_system_1", "Room_1_to_Booking_system_1"
                });
            await cl.CommandCreateRelationship(new string[5]
                {
                    "CreateEdge", "Plant_1", "contains", "CNC_machine_1", "Plant_1_to_CNC_machine_1"
                });
            await cl.CommandCreateRelationship(new string[5]
                {
                    "CreateEdge", "Plant_1", "contains", "CNC_machine_2", "Plant_1_to_CNC_machine_2"
                });
            await cl.CommandCreateRelationship(new string[5]
                {
                    "CreateEdge", "CNC_machine_1", "contains", "Three_color_light_1", "CNC_machine_1_to_Three_color_light_1"
                });
            await cl.CommandCreateRelationship(new string[5]
                {
                    "CreateEdge", "CNC_machine_1", "contains", "Cutter_tool_1", "CNC_machine_1_to_Cutter_tool_1"
                });
        }
    }
}
