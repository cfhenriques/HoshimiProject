using System;
using System.Collections.Generic;
using System.Text;
using AASMAHoshimi;
using PH.Common;
using System.Drawing;

namespace AASMAHoshimi.Examples
{
    public class BuildingAI : AASMAAI
    {

        public BuildingAI(NanoAI nanoAI) : base(nanoAI)
        {
        }
        

        public override void DoActions()
        {   
            if (this._nanoAI.State == NanoBotState.WaitingOrders)
            {

                if (getAASMAFramework().overHoshimiPoint(this._nanoAI) && 
                    !getAASMAFramework().overNeedle(this._nanoAI))
                    this._nanoAI.Build(typeof(PassiveNeedle), "N" + this._needleNumber++);
                else if (getAASMAFramework().protectorsAlive() < 4)
                {
                    int rand = Utils.randomValue(2);

                    if(rand == 0)
                        this._nanoAI.Build(typeof(RandomProtector), "RP" + this._protectorNumber++);
                    else // if(rand == 1)
                        this._nanoAI.Build(typeof(AIProtector), "AIP" + this._protectorNumber++);
                //    else if (rand == 2)
                //        this._nanoAI.Build(typeof(ContainerProtector), "CP" + this._protectorNumber++);
                //    else
                //        this._nanoAI.Build(typeof(NeedleProtector), "NP" + this._protectorNumber++);
                    
                }
                else if (getAASMAFramework().containersAlive() < 2)
                    this._nanoAI.Build(typeof(PassiveContainer), "C" + this._containerNumber++);
             /*   else if (getAASMAFramework().explorersAlive() < 2)
                    this._nanoAI.Build(typeof(ForwardExplorer), "E" + this._explorerNumber++);          */
                    
                else
                    Move();
                
            }
 
            


        }


        private void Move()
        {
            Point nearestHoshimiPoint = Point.Empty;
            int distToHoshimiPoint = Int16.MaxValue;

            foreach (Point hoshPosition in getAASMAFramework().visibleHoshimies(this._nanoAI))
            {
                if (Utils.SquareDistance(this._nanoAI.Location, hoshPosition) < distToHoshimiPoint &&
                    !getAASMAFramework().visibleFullNeedles(this._nanoAI).Contains(hoshPosition) &&
                    !getAASMAFramework().visibleEmptyNeedles(this._nanoAI).Contains(hoshPosition))
                {
                    distToHoshimiPoint = Utils.SquareDistance(this._nanoAI.Location, hoshPosition);
                    nearestHoshimiPoint = hoshPosition;
                }
            }

            if (!nearestHoshimiPoint.IsEmpty && getAASMAFramework().isMovablePoint(nearestHoshimiPoint))
                this._nanoAI.MoveTo(nearestHoshimiPoint);
            else if (frontClear())
                this.MoveForward();
            else
                this.RandomTurn();
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
