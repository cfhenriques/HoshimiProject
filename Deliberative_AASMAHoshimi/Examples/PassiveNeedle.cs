using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using PH.Common;
using System.Diagnostics;

namespace Deliberative_AASMAHoshimi.Examples
{
    //a needle can't do much since it cannot move. However, it does have vision (so it can see enemies and other stuff),
    //can attack Pierre Bots and can send messages. 
    [Characteristics(ContainerCapacity = 100, CollectTransfertSpeed = 0, Scan = 10, MaxDamage = 5, DefenseDistance = 10, Constitution = 25)]
    public class PassiveNeedle : AASMANeedle
    {

        List<Instruction> currentPlan = new List<Instruction>();

        enum Desire
        {
            EMPTY,
            SEARCH_PIERRE,
            ATTACK
        }

        enum Instructions
        {
            DO_NOTHING,
            ATTACK
        }

        struct Intention
        {
            private Desire desire;
            private Point pierre;
            public Intention (Desire _desire)
            {
                desire = _desire;
                pierre = Point.Empty;
            }

            public Intention(Desire _desire, Point _pierre)
            {
                desire = _desire;
                pierre = _pierre;
            }

            public Desire getDesire()
            {
                return desire;
            }

            public Point getPierre()
            {
                return pierre;
            }
        }

        struct Instruction
        {
            private Instructions instruction;
            private Point pierre;

            public Instruction(Instructions instr)
            {
                instruction = instr;
                pierre = Point.Empty;
            }

            public Instruction(Instructions instr, Point p)
            {
                instruction = instr;
                pierre = p;
            }

            public Instructions getInstruction()
            {
                return instruction;
            }

            public Point getPierre()
            {
                return pierre;
            }
        }


        public override void DoActions()
        {
            if (this.State == NanoBotState.WaitingOrders)
            {
                if (currentPlan.Count != 0)
                {
                    Execute(currentPlan);
                    UpdateBeliefs();


                }
                else
                {
                    UpdateBeliefs();

                    Desire d = Options();
                    Intention i = Filter(d);
                    currentPlan = Plan(i);
                }
            }
        }


        private void UpdateBeliefs()
        {
            // outbox
            if(Stock == ContainerCapacity)
            {
                AASMAMessage msg = new AASMAMessage(this.InternalName, "C_,E_$ FULL NEEDLE");
                msg.Tag = this.Location;
                getAASMAFramework().broadCastMessage(msg);
            }
        }

        private Desire Options()
        {
            foreach (Point pierre in this.getAASMAFramework().visiblePierres(this))
                if (Utils.SquareDistance(this.Location, pierre) <= this.DefenseDistance * this.DefenseDistance)
                    return Desire.ATTACK;

            return Desire.SEARCH_PIERRE;
        }

        private Intention Filter(Desire desire)
        {
            switch(desire)
            {
                case Desire.ATTACK:
                    foreach (Point p in this.getAASMAFramework().visiblePierres(this))
                        if (Utils.SquareDistance(this.Location, p) <= this.DefenseDistance * this.DefenseDistance)
                            return new Intention(desire, p);

                    Debug.Write( this.InternalName + " is trying to attack no one");
                    return new Intention(Desire.EMPTY);
                case Desire.SEARCH_PIERRE:
                    return new Intention(desire);
                default:
                    Debug.WriteLine(this.InternalName + " built an empty intention");
                    return new Intention(Desire.EMPTY);
            }
        }

        private List<Instruction> Plan(Intention intention)
        {
            List<Instruction> myplan = new List<Instruction>();

            switch (intention.getDesire())
            {
                case Desire.SEARCH_PIERRE:
                    myplan.Add(new Instruction(Instructions.DO_NOTHING));
                    break;
                case Desire.ATTACK:
                    myplan.Add(new Instruction(Instructions.ATTACK, intention.getPierre()));
                    break;
                default:
                    break;
            }

            return myplan;
        }

        private void Execute(List<Instruction> plan)
        {
            Instruction i = plan[0];
            plan.RemoveAt(0);

            switch (i.getInstruction())
            {
                case Instructions.DO_NOTHING:
                    break;
                case Instructions.ATTACK:
                    Debug.WriteLine(this.InternalName + " is attacking!!");
                    this.DefendTo(i.getPierre(), 2);
                    break;
                default:
                    break;
            }
        }


        public override void receiveMessage(AASMAMessage msg)
        {
        }
    }
}
