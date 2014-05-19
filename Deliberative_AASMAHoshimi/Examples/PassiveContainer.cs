using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PH.Common;

namespace Deliberative_AASMAHoshimi.Examples
{
    //this is an example of a stupid collector that does not move, however if he is under an AZN point he will try to collect
    //something
    [Characteristics(ContainerCapacity = 50, CollectTransfertSpeed = 5, Scan = 0, MaxDamage = 0, DefenseDistance = 0, Constitution = 15)]
    class PassiveContainer : AASMAContainer
    {
        
        enum Desire
        {
            EMPTY,
            SEARCH_AZNPOINT,
            GOTO_AZN,
            SEARCH_NEEDLE,
            GOTO_NEEDLE
        };

        enum Instructions
        {
            MOVE,
            MOVE_TO,
            COLLECT,
            DROP
        }

        struct Intention
        {
            private Desire _desire;
            private Point _dest;

            public Intention(Desire d, Point p)
            {
                _desire = d;
                _dest = p;
            }

            public Desire getDesire()
            {
                return _desire;
            }

            public Point getDest()
            {
                return _dest;
            }
        };

        struct Instruction
        {
            private Instructions instruction;
            private Point dest;

            public Instruction(Instructions instr, Point p)
            {
                instruction = instr;
                dest = p;
            }

            public Instruction(Instructions instr)
            {
                instruction = instr;
                dest = Point.Empty;
            }

            public Instructions getInstruction()
            {
                return instruction;
            }

            public Point getDest()
            {
                return dest;
            }
        }

        List<Instruction> currentPlan = new List<Instruction>();
        Intention currentIntention = new Intention(Desire.EMPTY, Point.Empty);



        public override void DoActions()
        {
            if (this.State == NanoBotState.WaitingOrders)
            {
                if (currentPlan.Count != 0 || !Succeeded(currentIntention))
                {
                    Execute(currentPlan);
                    UpdateBeliefs();
                }
                else
                {
                    UpdateBeliefs();

                    Desire d = Options();
                    currentIntention = Filter(d);
                    currentPlan = Plan(currentIntention);
                }
            }
        }

        private void UpdateBeliefs()
        {

        }

        private Desire Options()
        {
            if (this.Stock.Equals(this.ContainerCapacity)) // full
            {
                foreach (Point p in getAASMAFramework().visibleEmptyNeedles(this))
                    return Desire.GOTO_NEEDLE;

                return Desire.SEARCH_NEEDLE;
            }
            else //empty
            {
                foreach (Point p in getAASMAFramework().visibleAznPoints(this))
                    return Desire.GOTO_AZN;

                return Desire.SEARCH_AZNPOINT;
            }

        }

        private Intention Filter(Desire desire)
        {
            switch(desire)
            {
                case Desire.SEARCH_AZNPOINT:
                case Desire.SEARCH_NEEDLE:
                    return new Intention(desire, Point.Empty);
                case Desire.GOTO_AZN:
                    foreach (Point p in getAASMAFramework().visibleAznPoints(this))
                        return new Intention(desire, p); 

                    Debug.WriteLine(this.InternalName + " tried to go to an empty azn point");
                    return new Intention(Desire.EMPTY, Point.Empty);
                case Desire.GOTO_NEEDLE:
                    foreach (Point p in getAASMAFramework().visibleEmptyNeedles(this))
                        return new Intention(desire, p); 

                    Debug.WriteLine(this.InternalName + " tried to go to an inexistent needle");
                    return new Intention(Desire.EMPTY, Point.Empty);

                default:
                    return new Intention(Desire.EMPTY, Point.Empty);
            }
        }

        private List<Instruction> Plan(Intention intention)
        {
            List<Instruction> myplan = new List<Instruction>();

            switch (intention.getDesire())
            {
                case Desire.SEARCH_AZNPOINT:
                case Desire.SEARCH_NEEDLE:
                    myplan.Add(new Instruction(Instructions.MOVE));
                    break;
                case Desire.GOTO_AZN:
                    myplan.Add(new Instruction(Instructions.MOVE_TO, intention.getDest()));
                    myplan.Add(new Instruction(Instructions.COLLECT));
                    break;
                case Desire.GOTO_NEEDLE:
                    myplan.Add(new Instruction(Instructions.MOVE_TO, intention.getDest()));
                    myplan.Add(new Instruction(Instructions.DROP));
                    break;
                default:
                    break;
            }

            return myplan;
        }

        private void Execute(List<Instruction> plan)
        {
            Instruction i = new Instruction();

            if (plan.Count > 0)
            {
                i = plan[0];
                plan.RemoveAt(0);
            }
            else if (currentIntention.getDesire() == Desire.GOTO_AZN && this.getAASMAFramework().overAZN(this))
                i = new Instruction(Instructions.COLLECT);
            else if (currentIntention.getDesire() == Desire.GOTO_NEEDLE && this.getAASMAFramework().overEmptyNeedle(this))
                i = new Instruction(Instructions.DROP);
                


            switch (i.getInstruction())
            {
                case Instructions.MOVE_TO:
                    this.MoveTo(i.getDest());
                    break;
                case Instructions.MOVE:
                    if (frontClear())
                        MoveForward();
                    else
                        RandomTurn();
                    break;
                case Instructions.COLLECT:
                    if (this.getAASMAFramework().overAZN(this))
                        this.collectAZN();
                    else
                        Debug.WriteLine(this.InternalName + " tried to collect AZN when far from AZN Point");
                    break;
                case Instructions.DROP:
                    if (this.getAASMAFramework().overEmptyNeedle(this))
                        this.transferAZN();
                    else
                        Debug.WriteLine(this.InternalName + " tried to collect AZN when far from Needle");
                    break;
                default:
                    break;
            }

        }

        private bool Succeeded(Intention i)
        {
            if (i.getDesire() == Desire.GOTO_AZN && this.getAASMAFramework().overAZN(this) && this.Stock < this.ContainerCapacity)
                return false;
            else if (i.getDesire() == Desire.GOTO_NEEDLE && this.getAASMAFramework().overEmptyNeedle(this) && this.Stock != 0)
                return false;


            return true;
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }
    }
}
