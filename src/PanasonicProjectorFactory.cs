using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using System.Collections.Generic;

namespace PepperDash.Essentials.Plugins.Display.Panasonic.Projector
{
    /// <summary>
    /// Plugin device factory for devices that use IBasicCommunication
    /// </summary>
    public class PanasonicProjectorFactory : EssentialsPluginDeviceFactory<PanasonicProjectorController>
    {
        /// <summary>
        /// Plugin device factory constructor
        /// </summary>
        public PanasonicProjectorFactory()
        {
            // Set the minimum Essentials Framework Version
            MinimumEssentialsFrameworkVersion = "1.11.1";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
            TypeNames = new List<string> { "panasonicProjector" };
        }

        /// <summary>
        /// Builds and returns an instance of EssentialsPluginDeviceTemplate
        /// </summary>
        /// <param name="dc">device configuration</param>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
#if SERIES4
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"[{dc.Key}] Factory Attempting to create new device from type: {dc.Type}", null, null);
#else
            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);
#endif
            // get the plugin device properties configuration object & check for null 
            var propertiesConfig = dc.Properties.ToObject<PanasonicProjectorConfig>();
            if (propertiesConfig == null)
            {
#if SERIES4
                Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"[{dc.Key}] Factory: failed to read properties config for {dc.Name}", null, null);
#else
                Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
#endif
                return null;
            }

            // attempt build the plugin device comms device & check for null
            var comms = CommFactory.CreateCommForDevice(dc);

            if (comms != null)
            {
                return new PanasonicProjectorController(dc.Key, dc.Name, propertiesConfig, comms);
            }
#if SERIES4
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"[{dc.Key}] Factory Notice: No control object present for device {dc.Name}", null, null);
#else
            Debug.Console(1, "[{0}] Factory Notice: No control object present for device {1}", dc.Key, dc.Name);
#endif
            return null;
        }
    }
}

