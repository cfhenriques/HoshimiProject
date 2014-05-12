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
        bool b = false;

        public BuildingAI(NanoAI nanoAI) : base(nanoAI)
        {
        }
        

        public override void DoActions()
        {
            /*
            //builds one nanobot of the type Explorer
            //the explorersAlive method gives us the number of nano explorers that are still alive (i.e. not destroyed)
            if (getAASMAFramework().explorersAlive() == 0)
                {
                //we want to identify explorers with the names E1,E2,E3,...
                //that's why we use the explorerNumber++;
                //if u don't want to give a name to the nanobot use the following constructor instead
                //this._nanoAi.Build(typeof(ForwardExplorer));
                //however, if you do this, you won't be able to send him personal messages
                this._nanoAI.Build(typeof(CommunicativeExplorer), "E" + this._explorerNumber++); 
            }
            
             */


            /*  [CH]
             *  Notes:
             *      1? in the reactive case, a ForwardExplorer is always created when there are no ForwardExplorers alive. In the remaining cases, a 
             *      ForwardExplorer is only created when there are unvisited navigation points
             *      
             *      2? in the reactive case, a ShootingProtector is always created when there are no ShootingProtectors alive. In the remaining cases, 
             *      a ShootingProtector is only created when there are enemies alive
             */
            
            if (this._nanoAI.State == NanoBotState.WaitingOrders)
            {
                if (getAASMAFramework().overHoshimiPoint(this._nanoAI) && 
                    !getAASMAFramework().overNeedle(this._nanoAI))
                    this._nanoAI.Build(typeof(PassiveNeedle), "N" + this._needleNumber++);
                else if (getAASMAFramework().protectorsAlive() < 2)
                    this._nanoAI.Build(typeof(ShootingProtector), "P" + this._protectorNumber++);
                else if (getAASMAFramework().explorersAlive() < 2)
                    this._nanoAI.Build(typeof(ForwardExplorer), "E" + this._explorerNumber++);
                else if (getAASMAFramework().containersAlive() < 2)
                    this._nanoAI.Build(typeof(PassiveContainer), "C" + this._containerNumber++);
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
