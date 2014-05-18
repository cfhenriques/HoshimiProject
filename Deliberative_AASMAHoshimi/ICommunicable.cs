using System;
using System.Collections.Generic;
using System.Text;

namespace Deliberative_AASMAHoshimi
{
    interface ICommunicable
    {
        void receiveMessage(AASMAMessage msg);
    }
}
