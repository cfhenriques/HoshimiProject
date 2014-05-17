using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;

namespace AASMAHoshimi.Examples
{
    //this protector does not move (u should write the code for it), however he will shoot any incoming pierre's that he sees
    //however, it is frequent that pierre's neurocontrollers kill the protector before he sees it
    //note that the shooting range is greater than the scan range
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 5, MaxDamage = 5, DefenseDistance = 12, Constitution = 28)]
    public class RandomProtector : AASMAProtector
    {
        public override void  DoActions()
        {
            if (this.State == NanoBotState.WaitingOrders)
            {
                if(hasVisibleEnemies())
                    AttackVisibleEnemies();
                else
                    Move();
                
            }
        }

        public void Move()
        {
            Point teamPierreInjPoint = getAASMAFramework().PierreTeamInjectionPoint;
            int dist = Utils.SquareDistance(this.Location, teamPierreInjPoint);

            int robotScanDistance = this.Scan + PH.Common.Utils.ScanLength;
            int sqrRobotScanDistance = robotScanDistance * robotScanDistance;


            if (dist < sqrRobotScanDistance)
                this.MoveTo(Utils.getPointInFront(teamPierreInjPoint, Utils.direction.NE));
            else if (frontClear())
                this.MoveForward();
            else
                this.RandomTurn();
        }

        private bool hasVisibleEnemies()
        {
            return getAASMAFramework().visiblePierres(this).Count > 0; 
        }

        public void AttackVisibleEnemies()
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
