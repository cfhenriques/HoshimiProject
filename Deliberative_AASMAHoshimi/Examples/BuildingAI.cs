using System;
using System.Collections.Generic;
using System.Text;
using Deliberative_AASMAHoshimi;
using PH.Common;
using System.Drawing;
using System.Diagnostics;

namespace Deliberative_AASMAHoshimi.Examples
{
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 5, MaxDamage = 0, DefenseDistance = 0, Constitution = 20)]
    public class BuildingAI : AASMAAI
    {
        List<Instruction> currentPlan = new List<Instruction>();
        Intention currentIntention = new Intention(Desire.EMPTY);

        List<AASMAMessage> inbox = new List<AASMAMessage>();

        List<Point> empty_hoshimi = new List<Point>();

        List<String> _AIProtectors = new List<String>();

        enum Desire
        {
            EMPTY,
            SEARCH_HOSHIMI,
            CREATE_ROBOTS
        }

        enum Instructions
        {
            MOVE,
            MOVE_TO_HOSHIMI,
            CREATE_CONTAINER,
            CREATE_NEEDLE,
            CREATE_AI_PROTECTOR,
            CREATE_CONTAINER_PROTECTOR,
            CREATE_NEEDLE_PROTECTOR,
            CREATE_RANDOM_PROTECTOR,
            CREATE_EXPLORER,
            DO_NOTHING
        }

        struct Intention
        {
            private Desire desire;
            private Point point;
            public Intention(Desire _desire)
            {
                desire = _desire;
                point = Point.Empty;
            }

            public Intention(Desire _desire, Point _point)
            {
                desire = _desire;
                point = _point;
            }

            public Desire getDesire()
            {
                return desire;
            }

            public Point getPoint()
            {
                return point;
            }
        }

        struct Instruction
        {
            private Instructions instruction;
            private Point point;

            public Instruction(Instructions instr)
            {
                instruction = instr;
                point = Point.Empty;
            }

            public Instruction(Instructions instr, Point p)
            {
                instruction = instr;
                point = p;
            }

            public Instructions getInstruction()
            {
                return instruction;
            }

            public Point getPoint()
            {
                return point;
            }
        }




        public BuildingAI(NanoAI nanoAI) : base(nanoAI)
        {
        }
        

        public override void DoActions()
        {
            if (! (currentPlan.Count == 0 || Succeeded(currentIntention)))
            {
                Execute(currentPlan);
                UpdateBeliefs();

                if(Reconsider())
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

            

            // inbox
            foreach(AASMAMessage msg in inbox)
            {
                if(msg.Content.Equals("AI$ AIPROTECTOR BEING KILLED"))
                    _AIProtectors.Remove(msg.Sender);
                else if(msg.Content.Equals("AI$ HOSHIMI POINT") && !empty_hoshimi.Contains((Point)msg.Tag))
                {
                    empty_hoshimi.Add((Point)msg.Tag);
              //      Debug.WriteLine(this._nanoAI.InternalName + ": hoshimi point found by " + msg.Sender + "    | Point = (" + ((Point)msg.Tag).X + "," + ((Point)msg.Tag).Y + ")");
                }
            }

            inbox.Clear();
        }

        private Desire Options()
        {
            List<Point> pierres = getAASMAFramework().visiblePierres(this._nanoAI);
            if (pierres.Count > 0 && _nanoAI.State == NanoBotState.Moving)
                return Desire.EMPTY;
            
            if(_AIProtectors.Count < 3)
                return Desire.CREATE_ROBOTS;

            if(!this.getAASMAFramework().overNeedle(this._nanoAI) )
            {
                foreach (Point p in this.getAASMAFramework().visibleHoshimies(this._nanoAI))
                    if (!getAASMAFramework().visibleFullNeedles(this._nanoAI).Contains(p) &&
                        !getAASMAFramework().visibleEmptyNeedles(this._nanoAI).Contains(p))
                        return Desire.CREATE_ROBOTS;

                if ( this.getAASMAFramework().explorersAlive() < 2 ||
                     this.getAASMAFramework().containersAlive() < 2)
                    return Desire.CREATE_ROBOTS;

                if (empty_hoshimi.Count > 0 && pierres.Count == 0)
                    return Desire.CREATE_ROBOTS;

            }

            if (getAASMAFramework().visiblePierres(this._nanoAI).Count > 0)
                return Desire.EMPTY;
            else
                return Desire.SEARCH_HOSHIMI; 
        }

        private Intention Filter(Desire desire)
        {
            switch(desire)
            {
                case Desire.CREATE_ROBOTS:
                    if(_AIProtectors.Count < 3)
                        return new Intention(desire);
                    
                    if (!this.getAASMAFramework().overNeedle(this._nanoAI)) 
                    {
                        foreach (Point p in this.getAASMAFramework().visibleHoshimies(this._nanoAI))
                            if ( !getAASMAFramework().visibleFullNeedles(this._nanoAI).Contains(p) &&
                                 !getAASMAFramework().visibleEmptyNeedles(this._nanoAI).Contains(p))
                                return new Intention(desire, p);

                        if (this.getAASMAFramework().explorersAlive() < 2 || this.getAASMAFramework().containersAlive() < 2)
                            return new Intention(desire);

                        if (empty_hoshimi.Count > 0)
                            return new Intention(desire, Utils.getNearestPoint(this._nanoAI.Location, empty_hoshimi));
                    }

                    


                    return new Intention(desire);
                case Desire.SEARCH_HOSHIMI:
                    return new Intention(desire);

                case Desire.EMPTY:
                    return new Intention(desire);
                default:
                    return new Intention(Desire.EMPTY);
            }
        }

        private List<Instruction> Plan(Intention intention)
        {
            List<Instruction> myplan = new List<Instruction>();

            switch (intention.getDesire())
            {
                case Desire.CREATE_ROBOTS: // + AI PROTECTORS
                    if (!intention.getPoint().IsEmpty)
                    {
                        myplan.Add(new Instruction(Instructions.MOVE_TO_HOSHIMI, intention.getPoint()));
                        myplan.Add(new Instruction(Instructions.CREATE_NEEDLE, intention.getPoint()));
                        // + needle protect
                        myplan.Add(new Instruction(Instructions.CREATE_NEEDLE_PROTECTOR));
                    }

                    else if(_AIProtectors.Count < 3) 
                        myplan.Add(new Instruction(Instructions.CREATE_AI_PROTECTOR));

                    else if (this.getAASMAFramework().containersAlive() < 2)
                        myplan.Add(new Instruction(Instructions.CREATE_CONTAINER));
                        // + container protectors
                    else if (this.getAASMAFramework().explorersAlive() < 2)
                        myplan.Add(new Instruction(Instructions.CREATE_EXPLORER));
                    else
                        myplan.Add(new Instruction(Instructions.CREATE_RANDOM_PROTECTOR));

                    break;
                case Desire.SEARCH_HOSHIMI:
                    myplan.Add(new Instruction(Instructions.MOVE));
                    break;
                case Desire.EMPTY:
                    myplan.Add(new Instruction(Instructions.DO_NOTHING));
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
                    if(_nanoAI.State == NanoBotState.Moving)
                        this._nanoAI.StopMoving();

                    break;
                case Instructions.MOVE:
                    if (frontClear())
                        MoveForward();
                    else
                        RandomTurn();
                    break;

                case Instructions.MOVE_TO_HOSHIMI:
                  /*  if (this.getAASMAFramework().visiblePierres(this._nanoAI).Count > 0) 
                    {
                        Debug.WriteLine(this._nanoAI.InternalName + " is seeing pierres");
                        Debug.WriteLine(this._nanoAI.InternalName + " state 1: " + this._nanoAI.State);
                        if (this._nanoAI.State == NanoBotState.Moving)
                            
                        Debug.WriteLine(this._nanoAI.InternalName + " state 2: " + this._nanoAI.State);
                    }
                    else
                    { */
                        AASMAMessage msg = new AASMAMessage(this._nanoAI.InternalName, "AIP_$ MOVE TO HOSHIMI");
                        msg.Tag = i.getPoint();
                        getAASMAFramework().broadCastMessage(msg);

                        this._nanoAI.MoveTo(i.getPoint());
                   // }
                    break;

                case Instructions.CREATE_AI_PROTECTOR:
                    this._nanoAI.Build(typeof(AIProtector), "AIP" + this._protectorNumber);
                    _AIProtectors.Add("AIP" + this._protectorNumber++);
                    break;
                case Instructions.CREATE_CONTAINER_PROTECTOR:
                    this._nanoAI.Build(typeof(ContainerProtector), "CP" + this._protectorNumber++);
                    break;
                case Instructions.CREATE_NEEDLE_PROTECTOR:
                    String protectorName = "NP" + this._protectorNumber++;
                    this._nanoAI.Build(typeof(NeedleProtector), protectorName);
                    // mandar mensagem com o ponto do needle
                    AASMAMessage msg_to_needle = new AASMAMessage(this._nanoAI.InternalName, "NP_$ MOVE TO NEEDLE LOCATION");
                    msg_to_needle.Tag = this._nanoAI.Location;
                    getAASMAFramework().sendMessage(msg_to_needle, protectorName);
                    break;
                case Instructions.CREATE_RANDOM_PROTECTOR:
                    this._nanoAI.Build(typeof(RandomProtector), "RP" + this._protectorNumber++);
                    break;

                case Instructions.CREATE_EXPLORER:
                    this._nanoAI.Build(typeof(ForwardExplorer), "E" + this._explorerNumber++);
                    break;

                case Instructions.CREATE_CONTAINER:
                    this._nanoAI.Build(typeof(PassiveContainer), "C" + this._containerNumber);

                    if(getAASMAFramework().explorersAlive() > 2)
                    {
                        AASMAMessage msg_1 = new AASMAMessage(this._nanoAI.InternalName, "E_$ CONTAINER CREATED:C" + this._containerNumber);
                        msg_1.Tag = i.getPoint();
                        getAASMAFramework().broadCastMessage(msg_1);
                    }
                    this._containerNumber++;
                    break;

                case Instructions.CREATE_NEEDLE:
                    if (getAASMAFramework().overHoshimiPoint(this._nanoAI) &&
                         !getAASMAFramework().overNeedle(this._nanoAI))
                    {
                        this._nanoAI.Build(typeof(PassiveNeedle), "N" + this._needleNumber++);
                        empty_hoshimi.Remove(this._nanoAI.Location);
                    }
                    break;

                default: break;
            }
        }

        private bool Reconsider() // should receive beleifs and intentions
        {
            List<Point> pierres = this.getAASMAFramework().visiblePierres(this._nanoAI);
            if (pierres.Count > 0)
            {
            //    Debug.WriteLine(this._nanoAI.InternalName + " reconsidered stopping");
            //    plan.Insert(0, new Instruction(Instructions.DO_NOTHING));
                return true;
            }

            return false;
        }

        private bool Succeeded(Intention intention)
        {
        //    Debug.Write("AI Succeeded: ");
            /*if (currentPlan.Count != 0)
            {
                return false;
            }*/
            if (intention.getDesire().Equals(Desire.CREATE_ROBOTS) /*&& !intention.getPoint().IsEmpty*/)
            {
                if ( this.getAASMAFramework().overHoshimiPoint(this._nanoAI) && 
                     this.getAASMAFramework().overNeedle(this._nanoAI))
                {

                    if (!intention.getPoint().IsEmpty && empty_hoshimi.Contains(intention.getPoint()))
                    {
                        return false;
                    }
                    else if (currentPlan.Count != 0)
                        return false;

                    return true;
                }
                else
                {
                    return false;
                }
                    


            }

            if (currentPlan.Count != 0)
            {
                return false;
            }

            return true;
        }

        public override void receiveMessage(AASMAMessage msg)
        {
            //this AI also handles messages, writing them in the debug log file
            //the logData method is very usefull for debbuging purposes
            //it will write the turn number and name of the agent who wrote in the log
       //     getAASMAFramework().logData(this._nanoAI, "received message from " + msg.Sender + " : " + msg.Content);
            if(msg.Content.Contains(_nanoAI.InternalName))
                inbox.Add(msg);
        }
    }
}
