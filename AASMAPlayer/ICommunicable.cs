using System;
using System.Collections.Generic;
using System.Text;

namespace Reactive_AASMAHoshimi
{
    interface ICommunicable
    {
        void receiveMessage(AASMAMessage msg);
    }
}
