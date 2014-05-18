using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Map;
using System.Diagnostics;

namespace AASMAHoshimi
{
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 5, MaxDamage = 5, DefenseDistance = 12, Constitution = 28)]
    public abstract class AASMAProtector : PH.Common.NanoCollector, IActionable, ICommunicable
    {
        private Utils.direction _direction;

        public AASMAProtector()
        {
            this._direction = Utils.RandomDirection();
        }

        public abstract void receiveMessage(AASMAMessage msg);

        public abstract void DoActions();

        public AASMAPlayer getAASMAFramework()
        {
            return (AASMAPlayer)this.PlayerOwner;
        }

        public Boolean frontClear()
        {
            Point p = Utils.getPointInFront(this.Location, this._direction);
            return Utils.isPointOK(PlayerOwner.Tissue, p.X, p.Y);
        }

        public bool canAttack()
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

        public void AttackEnemy()
        {
            int sqrDefenceDistance, sqrDistanceToEnemy;


            foreach (Point enemyPosition in getAASMAFramework().visiblePierres(this))
            {
                sqrDefenceDistance = this.DefenseDistance * this.DefenseDistance;
                sqrDistanceToEnemy = Utils.SquareDistance(this.Location, enemyPosition);

                if (sqrDistanceToEnemy < sqrDefenceDistance)
                {
                    Debug.WriteLine(this.InternalName + " Attacking Enemy!!!!!!!!");
                    this.DefendTo(enemyPosition, 2);
                    return;
                }
            }

        }

        public void MoveForward()
        {
            Point p = Utils.getPointInFront(this.Location, this._direction);
            this.MoveTo(p);
        }

        public void TurnLeft()
        {
            this._direction = Utils.DirectionLeft(this._direction);
        }

        public void TurnRight()
        {
            this._direction = Utils.DirectionRight(this._direction);
        }

        public void RandomTurn()
        {
            if (Utils.randomValue(2) == 1)
            {
                this._direction = Utils.DirectionLeft(this._direction);
            }
            else
            {
                this._direction = Utils.DirectionRight(this._direction);
            }
        }

    }
}
