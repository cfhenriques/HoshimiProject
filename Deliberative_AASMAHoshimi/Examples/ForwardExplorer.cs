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
        List<Instruction> currentPlan = new List<Instruction>();
        Intention currentIntention;


        List<Point> reachedNavPoints = new List<Point>();
        List<Point> navPointToReach = new List<Point>();

        List<Point> hoshimi_points = new List<Point>();
        List<Point> azn_points = new List<Point>();
        List<Point> empty_needles = new List<Point>();

        List<AASMAMessage> inbox = new List<AASMAMessage>();

        enum Desire
        {
            EMPTY,
            SEARCH_NAV_POINTS,
            GO_TO_NAV_POINTS
        };

        enum Instructions
        {
            MOVE,
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
            if (!(currentPlan.Count == 0 || Succeeded(currentIntention)))
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
        }


        private void UpdateBeliefs()
        {
       //     Debug.WriteLine(this.InternalName + " UpdateBeliefs");
            // if seeing navigation points
            foreach (Point p in this.getAASMAFramework().visibleNavigationPoints(this))
            {
                if (p.X == this.Location.X && p.Y == this.Location.Y && !reachedNavPoints.Contains(p))
                    reachedNavPoints.Add(p);
                
                if (!navPointToReach.Contains(p) && !reachedNavPoints.Contains(p))
                    navPointToReach.Add(p);
            }




            // inbox
            foreach(AASMAMessage msg in inbox)
            {
                if(msg.Content.Equals("E_$ NAVIGATION POINT REACHED"))
                {
                    Point p = (Point)msg.Tag;
                    if (!reachedNavPoints.Contains(p))
                        reachedNavPoints.Add(p);

                    if (navPointToReach.Contains(p))
                        navPointToReach.Remove(p);
                }

                else if (msg.Content.Equals("C_,E_$ FULL NEEDLE"))
                {
                    Point p = (Point)msg.Tag;
                    if (empty_needles.Contains(p))
                        empty_needles.Remove(p);

                    if (hoshimi_points.Contains(p))
                        hoshimi_points.Remove(p);

                }
                else if (msg.Content.Contains("E_$ CONTAINER CREATED"))
                {
                    String receiver = msg.Content.Split(new char[] { ':' })[1];

                    AASMAMessage new_msg = new AASMAMessage(this.InternalName, receiver + "$ AZN POINTS");
                    new_msg.Tag = azn_points;
                    getAASMAFramework().sendMessage(new_msg, receiver);

                    new_msg = new AASMAMessage(this.InternalName, receiver + "$ EMPTY NEEDLES");
                    new_msg.Tag = empty_needles;
                    getAASMAFramework().sendMessage(new_msg, receiver);
                }
            }

            // outbox
            //      if seeing hoshimi points
            foreach(Point hoshimi in this.getAASMAFramework().visibleHoshimies(this))
                if( !hoshimi_points.Contains(hoshimi) && 
                    !getAASMAFramework().visibleFullNeedles(this).Contains(hoshimi) &&
                    !getAASMAFramework().visibleEmptyNeedles(this).Contains(hoshimi))
                {
                    hoshimi_points.Add(hoshimi);

                    AASMAMessage msg = new AASMAMessage(this.InternalName, "AI$ HOSHIMI POINT");
                    msg.Tag = hoshimi;
                    getAASMAFramework().sendMessage(msg, "AI");
                }
                


            foreach(Point azn in getAASMAFramework().visibleAznPoints(this))
                if(!azn_points.Contains(azn))
                {
                    azn_points.Add(azn);

                    AASMAMessage msg = new AASMAMessage(this.InternalName, "C_$ AZN POINT");
                    msg.Tag = azn;
                    getAASMAFramework().broadCastMessage(msg);
                }

            foreach(Point needle in getAASMAFramework().visibleEmptyNeedles(this))
                if(!empty_needles.Contains(needle))
                {
                    empty_needles.Add(needle);
                    if(hoshimi_points.Contains(needle))
                        hoshimi_points.Remove(needle);

                    AASMAMessage msg = new AASMAMessage(this.InternalName, "C_$ EMPTY NEEDLE");
                    msg.Tag = needle;
                    getAASMAFramework().broadCastMessage(msg);
                }

            foreach(Point needle in getAASMAFramework().visibleFullNeedles(this))
            {
                if (hoshimi_points.Contains(needle))
                    hoshimi_points.Remove(needle);
            }
        }

        

        private Desire Options()
        {
            if (navPointToReach.Count > 0)
                return Desire.GO_TO_NAV_POINTS;

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
                        Point p = Point.Empty;
                        if (navPointToReach.Count > 0)
                            p = Utils.getNearestPoint(this.Location, navPointToReach);


                        return new Intention(Desire.GO_TO_NAV_POINTS, p);
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
                    myplan.Add(new Instruction(Instructions.MOVE));
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
            Instruction i;
            if (plan.Count == 0 && currentIntention.getDesire() == Desire.GO_TO_NAV_POINTS)
                i = new Instruction(Instructions.MOVE_TO_NAV_POINT, currentIntention.getDest());
            else
            {
                i = plan[0];
                plan.RemoveAt(0);
            }
           

           switch(i.getInstruction())
                {
                    case Instructions.MOVE_TO_NAV_POINT:
                        Point p = i.getDest();
                        if (this.State != NanoBotState.Moving)
                        {
                            navPointToReach.Remove(p);
                            reachedNavPoints.Add(p);

                            AASMAMessage msg = new AASMAMessage(this.InternalName, "E_$ NAVIGATION POINT REACHED");
                            msg.Tag = p;
                            getAASMAFramework().broadCastMessage(msg);

                            this.MoveTo(i.getDest());

                        }  
                        break;
                    case Instructions.MOVE:
                        //AASMAMessage msg = new AASMAMessage(this.InternalName, "oi");
                        //getAASMAFramework().sendMessage(msg, "AI");
                        if (frontClear())
                            MoveForward();
                        else
                            RandomTurn();
                        break;
                    default :
                        break;
                }

        }

        private bool Succeeded(Intention i)
        {
            if (i.getDesire() == Desire.GO_TO_NAV_POINTS && !i.getDest().Equals(this.Location))
                return false;

            if (currentPlan.Count != 0)
                return false;
           
            return true;
        }

        private bool Reconsider(Intention i)
        {
            List<Point> navPoints = getAASMAFramework().visibleNavigationPoints(this);
            if (i.getDesire() == Desire.GO_TO_NAV_POINTS && navPoints.Count > 0)
                foreach (Point nav in navPoints)
                    if (!reachedNavPoints.Contains(nav))
                        return true;

            return false;
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        //    getAASMAFramework().logData(this, " received message from " + msg.Sender + " : " + msg.Content);
            if (msg.Content.Contains("E_") || msg.Content.Contains(InternalName))
                inbox.Add(msg);
        }

    }
}
