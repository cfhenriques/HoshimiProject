using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PH.Common;

namespace AASMAHoshimi.Examples
{
    //this is an example of a stupid collector that does not move, however if he is under an AZN point he will try to collect
    //something
    [Characteristics(ContainerCapacity = 50, CollectTransfertSpeed = 5, Scan = 0, MaxDamage = 0, DefenseDistance = 0, Constitution = 15)]
    class PassiveContainer : AASMAContainer
    {
        public override void DoActions()
        {
            //stock is the ammount of azn the collector already has. If full, there is no point in collecting more azn.
            //the overAZN method checks if the received nanobot is over an AZN point
            if (this.State == NanoBotState.WaitingOrders)
            {
                if (Stock < ContainerCapacity && this.getAASMAFramework().overAZN(this))
                {
                    this.collectAZN();
                }
                else if (Stock > 0 && this.getAASMAFramework().overEmptyNeedle(this))
                {
                    this.transferAZN();
                }
                else
                    Move();
            }
        }

        private void Move()
        {
            int robotScanDistance = this.Scan + PH.Common.Utils.ScanLength;
            int sqrRobotScanDistance = robotScanDistance * robotScanDistance;

            int mindist = Int16.MaxValue;
            int dist ;

            Point destPoint = Point.Empty;

            if(this.Stock.Equals(this.ContainerCapacity))
                foreach (Point needle in getAASMAFramework().visibleEmptyNeedles(this))
                {
                    dist = Utils.SquareDistance(this.Location, needle);
                    if (dist < sqrRobotScanDistance)
                    {
                        mindist = dist;
                        destPoint = needle;
                    }
                }
            else
                foreach (Point aznPoint in getAASMAFramework().visibleAznPoints(this))
                {
                    dist = Utils.SquareDistance(this.Location, aznPoint);
                    if( dist < sqrRobotScanDistance)
                    {
                        mindist = dist;
                        destPoint = aznPoint;
                    }
                }
                

            if (!destPoint.IsEmpty) {
                System.Diagnostics.Debug.WriteLine("Container Capacity: " + this.NanoBotInfo.Stock);
                this.MoveTo(destPoint);
            }
            else if (frontClear())
                this.MoveForward();
            else
                this.RandomTurn();
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }
    }
}
