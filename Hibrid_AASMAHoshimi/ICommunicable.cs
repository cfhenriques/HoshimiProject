using System;
using System.Collections.Generic;
using System.Text;

namespace Hibrid_AASMAHoshimi
{
    interface ICommunicable
    {
        void receiveMessage(AASMAMessage msg);
    }
}
