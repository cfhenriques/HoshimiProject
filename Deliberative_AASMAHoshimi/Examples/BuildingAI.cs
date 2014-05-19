using System;
using System.Collections.Generic;
using System.Text;
using Deliberative_AASMAHoshimi;
using PH.Common;
using System.Drawing;
using System.Diagnostics;

namespace Deliberative_AASMAHoshimi.Examples
{
    public class BuildingAI : AASMAAI
    {
        List<Instruction> currentPlan = new List<Instruction>();

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
            CREATER_NEEDLE,
            CREATER_PROTECTOR,
            CREATE_EXPLORER
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
            if (this._nanoAI.State == NanoBotState.WaitingOrders)
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

        }

        private Desire Options()
        {

            if(!this.getAASMAFramework().overNeedle(this._nanoAI) )
                foreach (Point p in this.getAASMAFramework().visibleHoshimies(this._nanoAI))
                    if( !getAASMAFramework().visibleFullNeedles(this._nanoAI).Contains(p) &&
                        !getAASMAFramework().visibleEmptyNeedles(this._nanoAI).Contains(p))
                        return Desire.CREATE_ROBOTS;

            if (this.getAASMAFramework().explorersAlive() < 3 ||
                this.getAASMAFramework().containersAlive() < 3)
                return Desire.CREATE_ROBOTS;
            
            return Desire.SEARCH_HOSHIMI;
        }

        private Intention Filter(Desire desire)
        {
            switch(desire)
            {
                case Desire.CREATE_ROBOTS:
                    if (!this.getAASMAFramework().overNeedle(this._nanoAI))
                        foreach (Point p in this.getAASMAFramework().visibleHoshimies(this._nanoAI))
                            if ( !getAASMAFramework().visibleFullNeedles(this._nanoAI).Contains(p) &&
                                 !getAASMAFramework().visibleEmptyNeedles(this._nanoAI).Contains(p))
                                return new Intention(desire, p);


                    return new Intention(desire);
                case Desire.SEARCH_HOSHIMI:
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
                case Desire.CREATE_ROBOTS:
                    if (!intention.getPoint().IsEmpty)
                    {
                        myplan.Add(new Instruction(Instructions.MOVE_TO_HOSHIMI, intention.getPoint()));
                        myplan.Add(new Instruction(Instructions.CREATER_NEEDLE, intention.getPoint()));
                    }
                    else if (this.getAASMAFramework().containersAlive() < 3)
                        myplan.Add(new Instruction(Instructions.CREATE_CONTAINER));
                    else if (this.getAASMAFramework().explorersAlive() < 3)
                        myplan.Add(new Instruction(Instructions.CREATE_EXPLORER));
                    else ; // protectors ?

                    break;
                case Desire.SEARCH_HOSHIMI:
                    myplan.Add(new Instruction(Instructions.MOVE));
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
                case Instructions.MOVE:
                    if (frontClear())
                        MoveForward();
                    else
                        RandomTurn();
                    break;
                case Instructions.MOVE_TO_HOSHIMI:
                    this._nanoAI.MoveTo(i.getPoint());
                    break;
                case Instructions.CREATER_PROTECTOR:
                    // this._nanoAI.Build(typeof(RandomProtector), "RP" + this._protectorNumber++);
                    // this._nanoAI.Build(typeof(AIProtector), "AIP" + this._protectorNumber++);
                    // this._nanoAI.Build(typeof(ContainerProtector), "CP" + this._protectorNumber++);
                    // this._nanoAI.Build(typeof(NeedleProtector), "NP" + this._protectorNumber++);
                    break;
                case Instructions.CREATE_EXPLORER:
                    this._nanoAI.Build(typeof(ForwardExplorer), "E" + this._explorerNumber++);
                    break;
                case Instructions.CREATE_CONTAINER:
                    this._nanoAI.Build(typeof(PassiveContainer), "C" + this._containerNumber++);
                    break;
                case Instructions.CREATER_NEEDLE:
                    if (getAASMAFramework().overHoshimiPoint(this._nanoAI) &&
                         !getAASMAFramework().overNeedle(this._nanoAI))
                        this._nanoAI.Build(typeof(PassiveNeedle), "N" + this._needleNumber++);
                    break;
                default: break;
            }
        }

        public override void receiveMessage(AASMAMessage msg)
        {
            //this AI also handles messages, writing them in the debug log file
            //the logData method is very usefull for debbuging purposes
            //it will write the turn number and name of the agent who wrote in the log
            getAASMAFramework().logData(this._nanoAI, "received message from " + msg.Sender + " : " + msg.Content);
        }
    }
}
