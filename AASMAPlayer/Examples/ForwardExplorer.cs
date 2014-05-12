using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using PH.Common;

namespace AASMAHoshimi.Examples
{
    //this explorer always moves forward and turns when it cannot move. Uses the basic movement functions (turn, movefront)
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 30, MaxDamage = 0, DefenseDistance = 0, Constitution = 10)]
    public class ForwardExplorer : AASMAExplorer
    {
        List<Point> reachedNavPoints = new List<Point>();

        public override void DoActions()
        {
            if (this.State == NanoBotState.WaitingOrders)
                Move();
        }

        public void Move()
        {
            Point nearestNavPoint = Point.Empty;
            int distToNavPoint = Int16.MaxValue;

            foreach (Point navPosition in getAASMAFramework().visibleNavigationPoints(this))
            {
                if (!reachedNavPoints.Contains(navPosition) && Utils.SquareDistance(this.Location, navPosition) < distToNavPoint)
                {
                    distToNavPoint = Utils.SquareDistance(this.Location, navPosition);
                    nearestNavPoint = navPosition;
                }
            }

            if (!nearestNavPoint.IsEmpty && getAASMAFramework().isMovablePoint(nearestNavPoint))
            {
                reachedNavPoints.Add(nearestNavPoint);
                this.MoveTo(nearestNavPoint);
            }
            else if (frontClear())
                this.MoveForward();
            else
                this.RandomTurn();
        }

        public override void receiveMessage(AASMAMessage msg)
        {

        }

    }
}
