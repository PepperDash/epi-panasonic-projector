using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using System.Collections.Generic;

namespace PanasonicProjectorEpi
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
#if SERIES4
            // Set the minimum Essentials Framework Version
            MinimumEssentialsFrameworkVersion = "2.0.0";
#else
            // Set the minimum Essentials Framework Version
            MinimumEssentialsFrameworkVersion = "1.11.1";
#endif


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
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "[{key}] Factory Attempting to create new device from type: {type}", null, dc.Key, dc.Type);            
#else
            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);
#endif
            // get the plugin device properties configuration object & check for null 
            var propertiesConfig = dc.Properties.ToObject<PanasonicProjectorConfig>();
            if (propertiesConfig == null)
            {
#if SERIES4
                Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "[{key}] Factory: failed to read properties config for {name}", null, dc.Key, dc.Name);
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
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "[{key}] Factory Notice: No control object present for device {name}", null, dc.Key, dc.Name);
#else
            Debug.Console(1, "[{0}] Factory Notice: No control object present for device {1}", dc.Key, dc.Name);
#endif
            return null;
        }
    }
}

