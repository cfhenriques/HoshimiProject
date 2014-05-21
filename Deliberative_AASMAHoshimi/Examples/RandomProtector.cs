using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using System.Diagnostics;

namespace Deliberative_AASMAHoshimi.Examples
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
                if(canAttack())
                    AttackEnemy();
                else
                    Move();
                
            }
        }

        public void Move()
        {
            int sqrDefenceDistance, sqrDistanceToEnemy;

            foreach (Point enemy in getAASMAFramework().visiblePierres(this))
            {
                sqrDefenceDistance = this.DefenseDistance * this.DefenseDistance;
                sqrDistanceToEnemy = Utils.SquareDistance(this.Location, enemy);

                if (sqrDistanceToEnemy > sqrDefenceDistance)
                {
                    int x = this.Location.X;
                    int y = this.Location.Y;

                    if (this.Location.X < enemy.X)
                        x++;
                    else
                        x--;

                    if (this.Location.Y < enemy.Y)
                        y++;
                    else
                        y--;

                    Point dest = new Point(x, y);
                    if (getAASMAFramework().isMovablePoint(dest))
                    {
                        this.MoveTo(dest);
                  //      Debug.WriteLine(this.InternalName + " Moving towards enemy");
                        return;
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
