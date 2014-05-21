using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PH.Common;

namespace Deliberative_AASMAHoshimi.Examples
{
    [Characteristics(
        ContainerCapacity = 0, 
        CollectTransfertSpeed = 0, 
        Scan = 5, 
        MaxDamage = 5, 
        DefenseDistance = 12, 
        Constitution = 28)]
    public class NeedleProtector : AASMAProtector
    {
        protected override void UpdateBeliefs()
        {
            // verify inbox
            foreach (AASMAMessage msg in inbox)
            {
                if (msg.Content.Equals("NP_$ MOVE TO NEEDLE LOCATION"))
                {
                    protege = (Point)msg.Tag;
                }
            }
            inbox.Clear();
        }


        public override void receiveMessage(AASMAMessage msg)
        {
            if (msg.Content.Contains("NP_") || msg.Content.Contains(InternalName))
                inbox.Add(msg);
        }

    }
}
