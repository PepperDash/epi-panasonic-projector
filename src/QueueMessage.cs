using System;
using PepperDash.Essentials.Core.Queues;

namespace PanasonicProjectorEpi
{
    public class QueueMessage:IQueueMessage
    {
        private readonly Action _dispatchAction;

        public QueueMessage(Action dispatchAction)
        {
            _dispatchAction = dispatchAction;
        }

        public void Dispatch()
        {
            var handler = _dispatchAction;

            if (handler == null)
            {
                return;
            }

            handler();
        }
    }
}