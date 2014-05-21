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
    public class AIProtector : AASMAProtector
    {

        protected override void UpdateBeliefs()
        {
            if(HitPoint < 5)
            {
                AASMAMessage msg = new AASMAMessage(this.InternalName, "AI$ AIPROTECTOR BEING KILLED");
                getAASMAFramework().sendMessage(msg, "AI");
            }

            foreach (AASMAMessage msg in inbox)
            {
                if (msg.Content.Contains("AIP_$ MOVE TO HOSHIMI"))
                {
                    protege = (Point)msg.Tag;
                }

            }

            inbox.Clear();
        }


        protected override Intention Filter(Desire desire)
        {
            switch (desire)
            {
                case Desire.DEFEND_PROTEGE:
                    foreach (Point p in this.getAASMAFramework().visiblePierres(this))
                        if (Utils.SquareDistance(this.Location, p) <= this.DefenseDistance * this.DefenseDistance)
                            return new Intention(desire, p);

                    return new Intention(Desire.EMPTY);

                case Desire.GO_TO_PROTEGE:
                    
                    // inbox
                    if(!protege.IsEmpty)
                        return new Intention(Desire.GO_TO_PROTEGE, protege);


                    int robotScanDistance = this.Scan + PH.Common.Utils.ScanLength;
                    int sqrRobotScanDistance = robotScanDistance * robotScanDistance;
                    int sqrDistanceToAI = Utils.SquareDistance(this.Location, this.PlayerOwner.AI.Location);
                    if (sqrDistanceToAI < sqrRobotScanDistance)
                        return new Intention(Desire.GO_TO_PROTEGE, this.PlayerOwner.AI.Location);

                    //.Write(this.InternalName + " is trying to go to an inexistent AI");
                    return new Intention(Desire.EMPTY);
                
                case Desire.SEARCH_PROTEGE:
                    return new Intention(desire);
                
                default:
                 //   Debug.WriteLine(this.InternalName + " built an empty intention");
                    return new Intention(Desire.EMPTY);
            }
        }

        public override void receiveMessage(AASMAMessage msg)
        {
            inbox.Add(msg);
        }

    }
}
