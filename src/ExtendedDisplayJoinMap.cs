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
        public JoinDataComplete Connect;

        [JoinName("Disconnect")]
        public JoinDataComplete Disconnect;
    }
}