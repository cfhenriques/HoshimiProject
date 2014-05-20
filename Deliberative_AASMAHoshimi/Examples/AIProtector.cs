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
    public class AIProtector : AASMAProtector
    {

        List<Instruction> currentPlan = new List<Instruction>();
        Intention currentIntention = new Intention(Desire.EMPTY);

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
          /*    if(currentPlan.Count != 0)  */
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
            

        }

        private Desire Options()
        {
            foreach (Point pierre in this.getAASMAFramework().visiblePierres(this))
                if (Utils.SquareDistance(this.Location, pierre) <= this.DefenseDistance * this.DefenseDistance)
                    return Desire.DEFEND_PROTEGE;

            // inbox
            foreach (AASMAMessage msg in inbox)
            {
                if (msg.Content.Contains("AIP_$ MOVE TO HOSHIMI"))
                {
                    Debug.WriteLine(this.InternalName + " desired to move to hoshimi point");
                    return Desire.GO_TO_PROTEGE;
                }

            }

            int robotScanDistance = this.Scan + PH.Common.Utils.ScanLength;
            int sqrRobotScanDistance = robotScanDistance * robotScanDistance;
            int sqrDistanceToAI = Utils.SquareDistance(this.Location, this.PlayerOwner.AI.Location);
            if (sqrDistanceToAI < sqrRobotScanDistance)
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

                    Debug.Write(this.InternalName + " is trying to defend no one");
                    return new Intention(Desire.EMPTY);

                case Desire.GO_TO_PROTEGE:
                    
                    // inbox
                    AASMAMessage[] copy = new AASMAMessage[inbox.Count];
                    inbox.CopyTo(copy);
                    foreach (AASMAMessage msg in copy)
                    {
                        if (msg.Content.Contains("AIP_$ MOVE TO HOSHIMI"))
                        {
                            inbox.Remove(msg);
                            return new Intention(Desire.GO_TO_PROTEGE, (Point)msg.Tag);
                        }

                    }


                    int robotScanDistance = this.Scan + PH.Common.Utils.ScanLength;
                    int sqrRobotScanDistance = robotScanDistance * robotScanDistance;
                    int sqrDistanceToAI = Utils.SquareDistance(this.Location, this.PlayerOwner.AI.Location);
                    if (sqrDistanceToAI < sqrRobotScanDistance)
                        return new Intention(Desire.GO_TO_PROTEGE, this.PlayerOwner.AI.Location);

                    Debug.Write(this.InternalName + " is trying to go to an inexistent AI");
                    return new Intention(Desire.EMPTY);
                
                case Desire.SEARCH_PROTEGE:
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
                    Debug.WriteLine(this.InternalName+ " is moving randomly");
                    if (frontClear())
                        MoveForward();
                    else
                        RandomTurn();
                    break;
                case Instructions.MOVE_TO:
                    /*if (Utils.SquareDistance(this.Location, i.getPoint()) <= 18)
                    {*/
                        Debug.WriteLine(this.InternalName + " is moving to AI");
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
        /*
        private void Reconsider(List<Instruction> plan)
        {

            AASMAMessage[] copy = new AASMAMessage[inbox.Count];
            inbox.CopyTo(copy);
            foreach(AASMAMessage msg in copy)
            {
                if(msg.Content.Contains("AIP_$ MOVE TO HOSHIMI"))
                {
                    Debug.WriteLine(this.InternalName + " reconsidered to move to hoshimi point");
                    inbox.Remove(msg);

                    if (this.State == NanoBotState.Moving)
                        this.StopMoving();

                    //plan.Add(new Instruction(Instructions.MOVE_TO, (Point)msg.Tag));
                    plan.Insert(0, new Instruction(Instructions.MOVE_TO, (Point)msg.Tag));
                }

            }

            List<Point> pierres = this.getAASMAFramework().visiblePierres(this);
            if (pierres.Count > 0 && this.State == NanoBotState.Moving)
            {
                this.StopMoving();
                Debug.WriteLine(this.InternalName + " reconsidered to attack pierre");
                //plan.Add(new Instruction(Instructions.ATTACK, Utils.getNearestPoint(this.Location, pierres)));
                plan.Insert(0, new Instruction(Instructions.ATTACK, Utils.getNearestPoint(this.Location, pierres)));
            }
        }
        */
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
            inbox.Add(msg);
        }

    }
}
