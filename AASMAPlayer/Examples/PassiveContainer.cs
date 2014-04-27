using System;
using System.Collections.Generic;
using System.Text;
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
