using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core;

namespace PanasonicProjectorEpi
{
    public class ExtendedDisplayJoinMap : DisplayControllerJoinMap
    {
        [JoinName("Connect")]
        public JoinDataComplete Connect = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 255,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Connect Socket",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });


        public ExtendedDisplayJoinMap(uint joinStart)
            : base(joinStart, typeof(ExtendedDisplayJoinMap))
        {
        }
    }
}