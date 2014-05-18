using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using PH.Common;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web;

namespace Deliberative_AASMAHoshimi.Examples
{
    //this explorer always moves forward and turns when it cannot move. Uses the basic movement functions (turn, movefront)
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 30, MaxDamage = 0, DefenseDistance = 0, Constitution = 10)]
    public class ForwardExplorer : AASMAExplorer
    {

        List<Point> reachedNavPoints = new List<Point>();
        List<Instruction> currentPlan = new List<Instruction>();

        enum Desire
        {
            EMPTY,
            SEARCH_NAV_POINTS,
            GO_TO_NAV_POINTS
        };

        enum Instructions
        {
            MOVE_FORWARD,
            RANDOM_TURN,
            MOVE_TO_NAV_POINT,

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

            public Instruction (Instructions instr, Point p )
            {
                instruction = instr;
                dest = p;
            }

            public Instruction (Instructions instr)
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

        public override void DoActions()
        {
            if(currentPlan.Count != 0)
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


        private void UpdateBeliefs()
        {
            foreach (Point p in reachedNavPoints)
                Debug.WriteLine(this.InternalName + "     X: " + p.X + "   Y: " + p.Y);
            
            foreach (Point p in this.getAASMAFramework().visibleNavigationPoints(this))
                if (p.X == this.Location.X && p.Y == this.Location.Y && !reachedNavPoints.Contains(p))
                {
                    Debug.WriteLine(this.InternalName + " adding point x: " + this.Location.X + "   y: " + this.Location.Y);
                    reachedNavPoints.Add(p);
                }
                    
        }

        private Desire Options()
        {
            foreach (Point p in getAASMAFramework().visibleNavigationPoints(this))
            {
                if (!reachedNavPoints.Contains(p) && getAASMAFramework().isMovablePoint(p))
                    return Desire.GO_TO_NAV_POINTS;
            }

            return Desire.SEARCH_NAV_POINTS;
        }

        private Intention Filter(Desire desire)
        {
            switch(desire)
            {
                case Desire.SEARCH_NAV_POINTS:
                    return new Intention(Desire.SEARCH_NAV_POINTS, Point.Empty);

                case Desire.GO_TO_NAV_POINTS:
                    {
                        Point nearestNavPoint = Point.Empty;
                        int distToNavPoint = Int16.MaxValue;

                        foreach (Point navPosition in getAASMAFramework().visibleNavigationPoints(this))
                        {
                            if ( Utils.SquareDistance(this.Location, navPosition) < distToNavPoint &&
                                 getAASMAFramework().isMovablePoint(navPosition) &&
                                 !reachedNavPoints.Contains(navPosition))
                            {
                                distToNavPoint = Utils.SquareDistance(this.Location, navPosition);
                                nearestNavPoint = navPosition;
                            }
                        }

                        if (nearestNavPoint.IsEmpty)
                            Debug.WriteLine(this.InternalName + " tried to go to an empty point.");

                        return new Intention(Desire.GO_TO_NAV_POINTS, nearestNavPoint);
                    }
                default:
                    return new Intention(Desire.EMPTY, Point.Empty);
            }

        }


        private List<Instruction> Plan(Intention intention)
        {
            List<Instruction> myplan = new List<Instruction>();

            switch (intention.getDesire())
            {
                case Desire.SEARCH_NAV_POINTS:
                    if (frontClear())
                        myplan.Add(new Instruction(Instructions.MOVE_FORWARD));
                    else
                        myplan.Add(new Instruction(Instructions.RANDOM_TURN));
                    break;
                case Desire.GO_TO_NAV_POINTS:
                    myplan.Add(new Instruction(Instructions.MOVE_TO_NAV_POINT, intention.getDest()));
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

           switch(i.getInstruction())
                {
                    case Instructions.MOVE_TO_NAV_POINT:
                        this.MoveTo(i.getDest());
                        break;
                    case Instructions.MOVE_FORWARD:
                        MoveForward();
                        break;
                    case Instructions.RANDOM_TURN:
                        RandomTurn();
                        break;
                    default :
                        break;
                }

        }

        /*
        public void Move(Point p)
        { 
            if(p.IsEmpty)
            {
                if (frontClear())
                    this.MoveForward();
                else
                    this.RandomTurn();
            }
            else 
            {
                this.MoveTo(p);
            }


        }

        */

        public override void receiveMessage(AASMAMessage msg)
        {

        }

    }
}
