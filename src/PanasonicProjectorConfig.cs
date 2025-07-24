using Newtonsoft.Json;
using PepperDash.Essentials.Core;
using System.Collections.Generic;

namespace PanasonicProjectorEpi
{
    /// <summary>
    /// Plugin device configuration object
    /// </summary>	
    public class PanasonicProjectorConfig
	{
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig Control { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("warmupTimeInSeconds")]
        public long WarmupTimeInSeconds { get; set; }

        [JsonProperty("cooldownTimeInSeconds")]
        public long CooldownTimeInSeconds { get; set; }

        [JsonProperty("activeInputs")]
        public List<ActiveInputs> ActiveInputs { get; set; }
        }

    public class ActiveInputs
        {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public ActiveInputs()
            {
            Key = string.Empty;
            Name = string.Empty;
            }
        }
    }