using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PanasonicProjectorEpi
{
	/// <summary>
	/// Plugin device configuration object
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being created
	/// </remarks>
	[ConfigSnippet("\"properties\":{\"control\":{}")]
	public class PanasonicProjectorControllerConfig
	{
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig Control { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("warmupTimeInSeconds")]
        public long WarmupTimeInSeconds { get; set; }

        [JsonProperty("cooldownTimeInSeconds")]
        public long CooldownTimeInSeconds { get; set; }
	}
}