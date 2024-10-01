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
            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);

            // get the plugin device properties configuration object & check for null 
            var propertiesConfig = dc.Properties.ToObject<PanasonicProjectorConfig>();
            if (propertiesConfig == null)
            {
                Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
                return null;
            }

            // attempt build the plugin device comms device & check for null
            var comms = CommFactory.CreateCommForDevice(dc);

            if (comms != null)
            {
                return new PanasonicProjectorController(dc.Key, dc.Name, propertiesConfig, comms);
            }

            Debug.Console(1, "[{0}] Factory Notice: No control object present for device {1}", dc.Key, dc.Name);
            return null;
        }
    }
}

