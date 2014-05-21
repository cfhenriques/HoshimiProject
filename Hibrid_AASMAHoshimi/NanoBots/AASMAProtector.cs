using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Map;
using System.Diagnostics;

namespace Hibrid_AASMAHoshimi
{
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 5, MaxDamage = 5, DefenseDistance = 12, Constitution = 28)]
    public abstract class AASMAProtector : PH.Common.NanoCollector, IActionable, ICommunicable
    {
        private Utils.direction _direction;

        protected List<Instruction> currentPlan = new List<Instruction>();
        protected Intention currentIntention = new Intention(Desire.EMPTY);

        protected Point protege = Point.Empty;

        protected List<AASMAMessage> inbox = new List<AASMAMessage>();

        protected enum Desire
        {
            SEARCH_PROTEGE,
            DEFEND_PROTEGE,
            GO_TO_PROTEGE,
            EMPTY
        }

        protected enum Instructions
        {
            MOVE,
            MOVE_TO,
            ATTACK
        }

        protected struct Intention
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

        protected struct Instruction
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

        public AASMAProtector()
        {
            this._direction = Utils.RandomDirection();
        }

        public void DoActions()
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
        protected abstract void UpdateBeliefs();

        protected Desire Options()
        {
            foreach (Point pierre in this.getAASMAFramework().visiblePierres(this))
                if (Utils.SquareDistance(this.Location, pierre) <= this.DefenseDistance * this.DefenseDistance)
                    return Desire.DEFEND_PROTEGE;


            if (!protege.IsEmpty)
                return Desire.GO_TO_PROTEGE;

            return Desire.SEARCH_PROTEGE;
        }

        protected virtual Intention Filter(Desire desire)
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

                    if (!protege.IsEmpty)
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

        protected List<Instruction> Plan(Intention intention)
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

        protected void Execute(List<Instruction> plan)
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

        protected bool Reconsider(Intention i)
        {
            if (i.getDesire() == Desire.DEFEND_PROTEGE)
                return false;

            if (this.State == NanoBotState.Moving && this.getAASMAFramework().visiblePierres(this).Count > 0)
                return true;


            return false;
        }

        protected bool Succeeded(Intention i)
        {
            if (i.getDesire() == Desire.DEFEND_PROTEGE && this.getAASMAFramework().visiblePierres(this).Count > 0)
                return false;

            if (currentPlan.Count != 0)
                return false;

            return true;
        }

        

        public abstract void receiveMessage(AASMAMessage msg);

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
