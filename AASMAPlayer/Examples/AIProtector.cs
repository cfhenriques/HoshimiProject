using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PH.Common;

namespace AASMAHoshimi.Examples
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
        public override void DoActions()
        {
            //Debug.WriteLine(this.NanoBotInfo.InternalName + " DoActions");
            if(this.State == NanoBotState.WaitingOrders)
            {
                List<Point> enemies = this.getAASMAFramework().visiblePierres(this);
                if (enemies.Count > 0) {
                    foreach(Point p in enemies) {
                        if (AttackEnemy(p))
                            return;
                    }
                }
                
                Move();
            }
        }

        private void Move()
        {
            //Debug.WriteLine(this.NanoBotInfo.InternalName + " Move");
            int robotScanDistance = this.Scan + PH.Common.Utils.ScanLength;
            int sqrRobotScanDistance = robotScanDistance * robotScanDistance;

            { // searching for AI
                Point _AIlocation = this.PlayerOwner.AI.Location;
                int sqrDistanceToAI = Utils.SquareDistance(this.Location, _AIlocation);

                if (sqrDistanceToAI < sqrRobotScanDistance)
                {
                    Utils.direction randDir;

                    for (int i = 0; i < 4; i++)
                    {
                        randDir = Utils.RandomDirection();

                        if (getAASMAFramework().isMovablePoint(Utils.getPointInFront(_AIlocation, randDir)))
                        {
                            this.MoveTo((Utils.getPointInFront(_AIlocation, randDir)));
                            return;
                        }

                    }

                }
            }

            if (frontClear())
                this.MoveForward();
            else
                this.RandomTurn();
        }


        private bool AttackEnemy(Point p)
        {
            //Debug.WriteLine(this.NanoBotInfo.InternalName + " AtackEnemy");
            if (Utils.SquareDistance(this.Location, p) < (this.DefenseDistance * this.DefenseDistance))
            {
                this.DefendTo(p, 2);
                return true;
            }

            return false;
            
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }

    }
}
