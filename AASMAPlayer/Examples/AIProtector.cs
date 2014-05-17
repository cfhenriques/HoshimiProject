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
            Debug.WriteLine(this.NanoBotInfo.InternalName + " DoActions");
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
            Debug.WriteLine(this.NanoBotInfo.InternalName + " Move");
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
           // bool attack = true;
            Debug.WriteLine(this.NanoBotInfo.InternalName + " AtackEnemy");
            if (Utils.SquareDistance(this.Location, p) < (this.DefenseDistance * this.DefenseDistance))
            {
             /*   int my = p.Y - this.Location.Y;
                int mx = p.X - this.Location.X;
                int m;

                if (mx == 0 || my == 0)
                    m = 0;
                else
                    m = (p.Y - this.Location.Y)/(p.X - this.Location.X);

                int b = this.Location.Y - m * this.Location.X;

                

                if (m == 1 || m == -1)
                {
                    int minY = Math.Min(this.Location.Y, p.Y);
                    int maxY = Math.Max(this.Location.Y, p.Y);
                    for (int i = minY + 1; i < maxY; i++)
                        if (!this.getAASMAFramework().isMovablePoint(new Point((i - b)/m, i)))
                        {
                            attack = false;
                            break;
                        }
                }
                else
                {
                    int minX = Math.Min(this.Location.X, p.X);
                    int maxX = Math.Max(this.Location.X, p.X);
                    for (int i = minX + 1; i < maxX; i++)
                        if (!this.getAASMAFramework().isMovablePoint(new Point(i, m * i + b)))
                        {
                            attack = false;
                            break;
                        }
                }

                if (attack)
                {*/
                this.DefendTo(p, 2);
                return true;
                //}
            }

            return false;
            
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }

    }
}
