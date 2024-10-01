using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Crestron.SimplSharpPro.UI;

namespace PanasonicProjectorEpi
{
	/// <summary>
	/// Plugin device factory for devices that use IBasicCommunication
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being developed
	/// </remarks>
	/// <example>
	/// "PanasonicProjectorControllerFactory" renamed to "MyDeviceFactory"
	/// </example>
    public class PanasonicProjectorControllerFactory : EssentialsPluginDeviceFactory<PanasonicProjectorController>
    {
		/// <summary>
		/// Plugin device factory constructor
		/// </summary>
		/// <remarks>
		/// Update the MinimumEssentialsFrameworkVersion & TypeNames as needed when creating a plugin
		/// </remarks>
		/// <example>
 		/// Set the minimum Essentials Framework Version
		/// <code>
		/// MinimumEssentialsFrameworkVersion = "1.6.4;
        /// </code>
		/// In the constructor we initialize the list with the typenames that will build an instance of this device
        /// <code>
		/// TypeNames = new List<string>() { "SamsungMdc", "SamsungMdcDisplay" };
        /// </code>
		/// </example>
        public PanasonicProjectorControllerFactory()
        {
            // Set the minimum Essentials Framework Version
			MinimumEssentialsFrameworkVersion = "1.7.5";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
            TypeNames = new List<string> { "panasonicProjector" };
        }
        
		/// <summary>
		/// Builds and returns an instance of EssentialsPluginDeviceTemplate
		/// </summary>
		/// <param name="dc">device configuration</param>
		/// <returns>plugin device or null</returns>
		/// <remarks>		
		/// The example provided below takes the device key, name, properties config and the comms device created.
		/// Modify the EssetnialsPlugingDeviceTemplate constructor as needed to meet the requirements of the plugin device.
		/// </remarks>
		/// <seealso cref="PepperDash.Core.eControlMethod"/>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);

            // get the plugin device properties configuration object & check for null 
            var propertiesConfig = dc.Properties.ToObject<PanasonicProjectorControllerConfig>();
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

          