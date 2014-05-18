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
    public class ContainerProtector : AASMAProtector
    {
        public override void DoActions()
        {
            //Debug.WriteLine(this.NanoBotInfo.InternalName + " DoActions");
            if(this.State == NanoBotState.WaitingOrders)
            {
                if (canAttack())
                    AttackEnemy();
                else 
                    Move();
            }
        }

        private void Move()
        {
            int robotScanDistance = this.Scan + PH.Common.Utils.ScanLength;
            int sqrRobotScanDistance = robotScanDistance * robotScanDistance;

            { // searching for Container

                foreach(NanoBot bot in this.PlayerOwner.NanoBots)
                    if(bot is PassiveContainer)
                    {
                        int sqrDistanceToBot = Utils.SquareDistance(this.Location, bot.Location);

                        if (sqrDistanceToBot < sqrRobotScanDistance && sqrDistanceToBot <= 18)
                        {
                            Utils.direction randDir;
                            for (int i = 0; i < 4; i++)
                            {
                                randDir = Utils.RandomDirection();

                                if (getAASMAFramework().isMovablePoint(Utils.getPointInFront(bot.Location, randDir)))
                                {
                                //    Debug.WriteLine(this.NanoBotInfo.InternalName + " Moving towards Container");
                                    this.MoveTo((Utils.getPointInFront(bot.Location, randDir)));
                                    return;
                                }

                            }

                        }
                        else if(sqrDistanceToBot < sqrRobotScanDistance)
                        {
                            int x = this.Location.X;
                            int y = this.Location.Y;

                            if (this.Location.X < bot.Location.X)
                                x++;
                            else
                                x--;

                            if (this.Location.Y < bot.Location.Y)
                                y++;
                            else
                                y--;

                            Point dest = new Point(x, y);
                            if (getAASMAFramework().isMovablePoint(dest))
                            {
                                this.MoveTo(dest);
                                Debug.WriteLine(this.InternalName + " Moving towards Container");
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

        public override void receiveMessage(AASMAMessage msg)
        {
        }

    }
}
