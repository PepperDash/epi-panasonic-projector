using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PanasonicProjectorEpi
{
    public class PanasonicStatusMonitor : StatusMonitorBase
    {
        public PanasonicStatusMonitor(IKeyed parent, ICommunicationReceiver coms, long warningTime, long errorTime)
            : base(parent, warningTime, errorTime)
        {
            coms.TextReceived += (sender, args) =>
            {
                Status = MonitorStatus.IsOk;
                ResetErrorTimers();
            };
        }

        public override void Start()
        {
            StartErrorTimers();
        }

        public override void Stop()
        {
            StopErrorTimers();
        }
    }
}