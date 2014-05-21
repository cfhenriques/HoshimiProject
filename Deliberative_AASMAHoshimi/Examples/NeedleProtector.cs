using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PH.Common;

namespace Deliberative_AASMAHoshimi.Examples
{
    [Characteristics(
        ContainerCapacity = 0, 
        CollectTransfertSpeed = 0, 
        Scan = 5, 
        MaxDamage = 5, 
        DefenseDistance = 12, 
        Constitution = 28)]
    public class NeedleProtector : AASMAProtector
    {
        List<Instruction> currentPlan = new List<Instruction>();
        Intention currentIntention = new Intention(Desire.EMPTY);

        Point protege = Point.Empty;

        List<AASMAMessage> inbox = new List<AASMAMessage>();

        enum Desire
        {
            SEARCH_PROTEGE,
            DEFEND_PROTEGE,
            GO_TO_PROTEGE,
            EMPTY
        }

        enum Instructions
        {
            MOVE,
            MOVE_TO,
            ATTACK
        }

        struct Intention
        {
            private Desire desire;
            private Point pierre;
            public Intention(Desire _desire)
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

            public Point getPoint()
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

            public Point getPoint()
            {
                return pierre;
            }
        }


        public override void DoActions()
        {
            if (!(currentPlan.Count == 0 || Succeeded(currentIntention)))
            {
                Execute(currentPlan);
                UpdateBeliefs();

                if (Reconsider(currentIntention))
                {
                    Desire d = Options();
                    currentIntention = Filter(d);
                    currentPlan = Plan(currentIntention);
                }
            }
            else
            {
                UpdateBeliefs();

                Desire d = Options();
                currentIntention = Filter(d);
                currentPlan = Plan(currentIntention);
            }
        }

        private void UpdateBeliefs()
        {
            // verify inbox
            foreach (AASMAMessage msg in inbox)
            {
                if (msg.Content.Equals("NP_$ MOVE TO NEEDLE LOCATION"))
                {
                    protege = (Point)msg.Tag;
                }
            }
            inbox.Clear();
        }

        private Desire Options()
        {
            foreach (Point pierre in this.getAASMAFramework().visiblePierres(this))
                if (Utils.SquareDistance(this.Location, pierre) <= this.DefenseDistance * this.DefenseDistance)
                    return Desire.DEFEND_PROTEGE;

           
            if(!protege.IsEmpty)
                return Desire.GO_TO_PROTEGE;

            return Desire.SEARCH_PROTEGE;
        }

        private Intention Filter(Desire desire)
        {
            switch (desire)
            {
                case Desire.DEFEND_PROTEGE:
                    foreach (Point p in this.getAASMAFramework().visiblePierres(this))
                        if (Utils.SquareDistance(this.Location, p) <= this.DefenseDistance * this.DefenseDistance)
                            return new Intention(desire, p);

               //     Debug.Write(this.InternalName + " is trying to defend no one");
                    return new Intention(Desire.EMPTY);

                case Desire.GO_TO_PROTEGE:

                    if(!protege.IsEmpty)
                        return new Intention(Desire.GO_TO_PROTEGE, protege);

                  //  Debug.Write(this.InternalName + " is trying to go to an inexistent AI");
                    return new Intention(Desire.EMPTY);

                case Desire.SEARCH_PROTEGE:
                    return new Intention(desire);

                default:
                  //  Debug.WriteLine(this.InternalName + " built an empty intention");
                    return new Intention(Desire.EMPTY);
            }
        }


        private List<Instruction> Plan(Intention intention)
        {
            List<Instruction> myplan = new List<Instruction>();

            switch (intention.getDesire())
            {
                case Desire.DEFEND_PROTEGE:
                    myplan.Add(new Instruction(Instructions.ATTACK, intention.getPoint()));
                    break;

                case Desire.GO_TO_PROTEGE:
                    myplan.Add(new Instruction(Instructions.MOVE_TO, intention.getPoint()));
                    break;

                case Desire.SEARCH_PROTEGE:
                    myplan.Add(new Instruction(Instructions.MOVE));
                    break;

                default:
                    break;
            }
            return myplan;
        }


        private void Execute(List<Instruction> plan)
        {
            Instruction i;
            if (plan.Count == 0 && currentIntention.getDesire() == Desire.DEFEND_PROTEGE)
            {
                i = new Instruction(Instructions.ATTACK, currentIntention.getPoint());
            }
            else
            {
                i = plan[0];
                plan.RemoveAt(0);
            }


            switch (i.getInstruction())
            {
                case Instructions.MOVE:
                 //   Debug.WriteLine(this.InternalName + " is moving randomly");
                    if (frontClear())
                        MoveForward();
                    else
                        RandomTurn();
                    break;
                case Instructions.MOVE_TO:
                //    Debug.WriteLine(this.InternalName + " is moving to Needle");
                    Utils.direction randDir;
                    for (int j = 0; j < 4; j++)
                    {
                        randDir = Utils.RandomDirection();
                        if (getAASMAFramework().isMovablePoint(Utils.getPointInFront(i.getPoint(), randDir)))
                        {
                            this.MoveTo((Utils.getPointInFront(i.getPoint(), randDir)));
                            return;
                        }
                    }
                    /*}
                    else
                    {
                        Debug.WriteLine(this.InternalName + " is moving slowly to AI");

                        int x = this.Location.X;
                        int y = this.Location.Y;

                        if (this.Location.X < i.getPoint().X)
                            x++;
                        else
                            x--;

                        if (this.Location.Y < i.getPoint().Y)
                            y++;
                        else
                            y--;

                        Point dest = new Point(x, y);
                        if (getAASMAFramework().isMovablePoint(dest))
                        {
                            this.MoveTo(dest);
                            Debug.WriteLine(this.InternalName + " Moving towards AI");
                            return;
                        }
                    }*/
                    break;
                case Instructions.ATTACK:
                    if (this.State == NanoBotState.Moving)
                        this.StopMoving();

                    Debug.WriteLine(this.InternalName + " is attacking");
                    this.DefendTo(i.getPoint(), 2);
                    break;
                default:
                    break;
            }

        }


        private bool Reconsider(Intention i)
        {
            if (i.getDesire() == Desire.DEFEND_PROTEGE)
                return false;

            if (this.State == NanoBotState.Moving && this.getAASMAFramework().visiblePierres(this).Count > 0)
                return true;


            return false;
        }

        private bool Succeeded(Intention i)
        {
            if (i.getDesire() == Desire.DEFEND_PROTEGE && this.getAASMAFramework().visiblePierres(this).Count > 0)
            {
                return false;
            }

            if (currentPlan.Count != 0)
            {
                return false;

            }

            return true;
        }






        public override void receiveMessage(AASMAMessage msg)
        {
            if (msg.Content.Contains("NP_") || msg.Content.Contains(InternalName))
                inbox.Add(msg);
        }

    }
}
