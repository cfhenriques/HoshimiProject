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
            /*    List<Point> enemies = this.getAASMAFramework().visiblePierres(this);
                if (enemies.Count > 0) {
                    foreach(Point p in enemies) {
                        if (AttackEnemy(p))
                            return;
                    }
                }
             */
                if (canAttack())
                    AttackEnemy();
                else
                    Move();
            }
        }

        private bool canAttack()
        {

            int sqrDefenceDistance, sqrDistanceToEnemy;

            foreach (Point enemy in getAASMAFramework().visiblePierres(this))
            {
                sqrDefenceDistance = this.DefenseDistance * this.DefenseDistance;
                sqrDistanceToEnemy = Utils.SquareDistance(this.Location, enemy);

                if (sqrDistanceToEnemy <= sqrDefenceDistance)
                    return true;
            }

            return false;

        }

        private void Move()
        {
            //Debug.WriteLine(this.NanoBotInfo.InternalName + " Move");
            int robotScanDistance = this.Scan + PH.Common.Utils.ScanLength;
            int sqrRobotScanDistance = robotScanDistance * robotScanDistance;

            { // searching for AI
                Point _AIlocation = this.PlayerOwner.AI.Location;
                int sqrDistanceToAI = Utils.SquareDistance(this.Location, _AIlocation);

                if ( sqrDistanceToAI < sqrRobotScanDistance &&
                     sqrDistanceToAI <= 18 ) // 8 = 2^2 * 2^2;        // if he's near AI
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
                else if (sqrDistanceToAI < sqrRobotScanDistance)
                {
                    int x = this.Location.X;
                    int y = this.Location.Y;

                    if (this.Location.X < _AIlocation.X)
                        x++;
                    else
                        x--;

                    if (this.Location.Y < _AIlocation.Y)
                        y++;
                    else
                        y--;

                    Point dest = new Point(x,y);
                    if (getAASMAFramework().isMovablePoint(dest))
                    {
                        this.MoveTo(dest);
                        Debug.WriteLine(this.InternalName + " Moving towards AI");
                        return;
                    }
                }  
            }   

            if (frontClear())
                this.MoveForward();
            else
                this.RandomTurn();
        }


        private void AttackEnemy()
        {
            int sqrDefenceDistance, sqrDistanceToEnemy;


            foreach (Point enemyPosition in getAASMAFramework().visiblePierres(this))
            {
                sqrDefenceDistance = this.DefenseDistance * this.DefenseDistance;
                sqrDistanceToEnemy = Utils.SquareDistance(this.Location, enemyPosition);

                if (sqrDistanceToEnemy < sqrDefenceDistance)
                {
                    this.DefendTo(enemyPosition, 2);
                    return;
                }
            }
            
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }

    }
}
