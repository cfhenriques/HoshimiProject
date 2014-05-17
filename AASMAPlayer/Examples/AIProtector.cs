using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;

namespace AASMAHoshimi.Examples
{
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 5, MaxDamage = 5, DefenseDistance = 12, Constitution = 28)]
    public class AIProtector : AASMAProtector
    {
        public override void DoActions()
        {
            if(this.State == NanoBotState.WaitingOrders)
            {
                if (hasVisibleEnemies())
                    AttackVisibleEnemies();
                else
                    Move();
            }
        }

        private void Move()
        {

            Point _AIlocation = this.PlayerOwner.AI.Location;
            int sqrDistanceToAI = Utils.SquareDistance(this.Location, _AIlocation);

            int robotScanDistance = this.Scan + PH.Common.Utils.ScanLength;
            int sqrRobotScanDistance = robotScanDistance * robotScanDistance;

            if (sqrDistanceToAI < sqrRobotScanDistance)
            {
                this.MoveTo(new Point(_AIlocation.X - 2 , _AIlocation.Y - 2));
            }
            else if (frontClear())
                this.MoveForward();
            else
                this.RandomTurn();
        }

        private bool hasVisibleEnemies()
        {
            return getAASMAFramework().visiblePierres(this).Count > 0;
        }

        private void AttackVisibleEnemies()
        {
            int sqrDefenceDistance, sqrDistanceToEnemy;

            foreach (Point enemyPosition in getAASMAFramework().visiblePierres(this))
            {
                sqrDefenceDistance = this.DefenseDistance * this.DefenseDistance;
                sqrDistanceToEnemy = Utils.SquareDistance(this.Location, enemyPosition);

                //we need to test if the enemy is within firing distance.
                if (sqrDistanceToEnemy <= sqrDefenceDistance)
                {
                    //the defendTo commands fires to the specified position for a number of specified turns. 1 is the recommended number of turns.
                    this.DefendTo(enemyPosition, 1);
                }
            }
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }

    }
}
