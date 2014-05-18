using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using PH.Common;

namespace AASMAHoshimi.Examples
{
    //a needle can't do much since it cannot move. However, it does have vision (so it can see enemies and other stuff),
    //can attack Pierre Bots and can send messages. 
    [Characteristics(ContainerCapacity = 100, CollectTransfertSpeed = 0, Scan = 10, MaxDamage = 5, DefenseDistance = 10, Constitution = 25)]
    public class PassiveNeedle : AASMANeedle
    {
        public override void DoActions()
        {
            if (this.State == NanoBotState.WaitingOrders)
            {
                AttackVisibleEnemies();
            }
        }

        public void AttackVisibleEnemies()
        {
            int sqrDefenceDistance, sqrDistanceToEnemy;

            foreach (Point enemyPosition in getAASMAFramework().visiblePierres(this))
            {
                sqrDefenceDistance = this.DefenseDistance * this.DefenseDistance;
                sqrDistanceToEnemy = Utils.SquareDistance(this.Location, enemyPosition);

                if (sqrDistanceToEnemy <= sqrDefenceDistance)
                    this.DefendTo(enemyPosition, 2);
            }
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }
    }
}
