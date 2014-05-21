using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PH.Common;

namespace Hibrid_AASMAHoshimi.Examples
{
    [Characteristics(
        ContainerCapacity = 0, 
        CollectTransfertSpeed = 0, 
        Scan = 5, 
        MaxDamage = 5, 
        DefenseDistance = 12, 
        Constitution = 28)]
    public class ContainerProtector : AASMAProtector
    {
        protected override void UpdateBeliefs()
        {
            foreach(AASMAMessage msg in inbox)
            {
                if (msg.Content.Contains("$ Container's location"))
                    protege = (Point)msg.Tag;
            }
        }


        public override void receiveMessage(AASMAMessage msg)
        {
            if (msg.Content.Contains(InternalName) || msg.Content.Contains("CP_"))
                inbox.Add(msg);
        }

    }
}
