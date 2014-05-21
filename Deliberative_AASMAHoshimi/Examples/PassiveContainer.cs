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

        String myprotector;

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

        List<AASMAMessage> inbox = new List<AASMAMessage>();
        List<Point> empty_needles = new List<Point>();
        List<Point> azn_points = new List<Point>();

        public override void DoActions()
        {
                if (currentPlan.Count != 0 || !Succeeded(currentIntention))
                {
                    Execute(currentPlan);
                    UpdateBeliefs();

                    if(Reconsider(currentIntention))
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
            // }
        }

        private void UpdateBeliefs()
        {
            foreach(Point p in getAASMAFramework().visibleAznPoints(this))
                if(!azn_points.Contains(p))
                    azn_points.Add(p);

            foreach (Point p in getAASMAFramework().visibleEmptyNeedles(this))
                if (!empty_needles.Contains(p))
                    empty_needles.Add(p);

            // inbox
            foreach(AASMAMessage msg in inbox)
            {
                if (msg.Content.Equals("C_$ AZN POINT"))
                {
                    Point p = (Point)msg.Tag;
                    if (!azn_points.Contains(p))
                    {
                        azn_points.Add(p);
                        //   Debug.WriteLine(this.InternalName + " added an AZNPoint");
                    }
                }
                else if (msg.Content.Equals("C_$ EMPTY NEEDLE"))
                {
                    Point p = (Point)msg.Tag;
                    if (!empty_needles.Contains(p))
                    {
                        empty_needles.Add(p);
                        //   Debug.WriteLine(this.InternalName + " added an empty needle");
                    }
                }
                else if (msg.Content.Equals("C_,E_$ FULL NEEDLE"))
                {
                    Point p = (Point)msg.Tag;
                    if (empty_needles.Contains(p))
                    {
                        empty_needles.Remove(p);
                        //   Debug.WriteLine(this.InternalName + " removed an empty needle");
                    }
                }
                else if (msg.Content.Equals(InternalName + "$  AZN POINTS"))
                {
                    azn_points = (List<Point>)msg.Tag;
                }
                else if (msg.Content.Equals(InternalName + "$  EMPTY NEEDLES"))
                {
                    empty_needles = (List<Point>)msg.Tag;
                }
                else if (msg.Content.Contains(InternalName + "$ Protector number"))
                    myprotector = msg.Content.Split(new char[] { ':' })[1];

            }

        }

        private Desire Options()
        {
            if (this.Stock.Equals(this.ContainerCapacity)) // full
            {
                if(empty_needles.Count > 0)
                    return Desire.GOTO_NEEDLE;

             //   Debug.WriteLine("CONTAINER " + InternalName + " perdido (needles)");
                return Desire.SEARCH_NEEDLE;
            }
            else //empty
            {
                if (azn_points.Count > 0)
                    return Desire.GOTO_AZN;

             //   Debug.WriteLine("CONTAINER " + InternalName + " perdido (azn points)");
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
                    if (azn_points.Count > 0)
                        return new Intention(desire, Utils.getNearestPoint(this.Location, azn_points));

               //     Debug.WriteLine(this.InternalName + " tried to go to an empty azn point");
                    return new Intention(Desire.EMPTY, Point.Empty);
                case Desire.GOTO_NEEDLE:
                    if(empty_needles.Count > 0)
                        return new Intention(desire, Utils.getNearestPoint(this.Location, empty_needles)); 

               //     Debug.WriteLine(this.InternalName + " tried to go to an inexistent needle");
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
                //    Debug.WriteLine(this.InternalName + " plan -> move random");
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
                    Point p = i.getDest();
                    this.MoveTo(p);

                    AASMAMessage msg1 = new AASMAMessage(this.InternalName, myprotector + "$ Container's location");
                    msg1.Tag = p;
                    getAASMAFramework().sendMessage(msg1, myprotector);

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
                  //  else
                    //    Debug.WriteLine(this.InternalName + " tried to collect AZN when far from AZN Point");
                    break;
                case Instructions.DROP:
                    if (this.getAASMAFramework().overEmptyNeedle(this))
                        this.transferAZN();
                   // else
                   //     Debug.WriteLine(this.InternalName + " tried to collect AZN when far from Needle");
                    break;
                default:
                    break;
            }

        }

        private bool Succeeded(Intention i)
        {
            if (i.getDesire() == Desire.GOTO_AZN)
            {
                if(!this.getAASMAFramework().overAZN(this))
                    return false;
                else if(this.Stock < ContainerCapacity)
                    return false;

                //Debug.WriteLine(this.InternalName + " GOTO_AZN: " + this.getAASMAFramework().overAZN(this) + " : " + this.Stock);
                
            }
            else if (i.getDesire() == Desire.GOTO_NEEDLE)
            {
                if(!this.getAASMAFramework().overEmptyNeedle(this) && this.getAASMAFramework().overNeedle(this))
                    return true;
                else if(!this.getAASMAFramework().overEmptyNeedle(this))
                    return false;
                else if(this.Stock != 0)
                    return false;
                

                //Debug.WriteLine(this.InternalName + " GOTO_NEEDLE: " + this.getAASMAFramework().overEmptyNeedle(this) + " : " + this.Stock);
            }
                
            return true;
        }

        private bool Reconsider(Intention i)
        {

            if (i.getDesire() == Desire.GOTO_AZN && getAASMAFramework().visibleAznPoints(this).Count > 0 && !getAASMAFramework().overAZN(this))
            {
            //    if (State == NanoBotState.Moving)
            //        StopMoving();

                return true;
            }
                

            if (i.getDesire() == Desire.GOTO_NEEDLE && getAASMAFramework().visibleEmptyNeedles(this).Count > 0 && !getAASMAFramework().overEmptyNeedle(this))
            {
            //    if (State == NanoBotState.Moving)
            //        StopMoving();
                return true;
            }

            return false;
        }

        public override void receiveMessage(AASMAMessage msg)
        {
         //   getAASMAFramework().logData(this, "received message from " + msg.Sender + " : " + msg.Content);
            if(msg.Content.Contains("C_") || msg.Content.Contains(this.InternalName))
                inbox.Add(msg);

        }
    }
}
