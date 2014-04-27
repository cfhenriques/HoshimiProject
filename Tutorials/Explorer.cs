using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using PH.Common;
using PH.Map;

namespace Tutorials
{
    [Characteristics(ContainerCapacity = 0,

    CollectTransfertSpeed = 0,

    Scan = 30,

    MaxDamage = 0,

    DefenseDistance = 0,

    Constitution = 10)]
    class Explorer: PH.Common.NanoExplorer, IActionable
    {
        public enum WhatToDoNextAction
        {
            WaitingForPoints = 0,
            MoveToPoint = 1,
        }

        private WhatToDoNextAction m_WhatToDoNext = WhatToDoNextAction.WaitingForPoints;
        private Queue<Point> m_PointsToVisit = new Queue<Point>();

        public WhatToDoNextAction WhatToDoNext
        {
            get { return m_WhatToDoNext; }
            set { m_WhatToDoNext = value; }
        }

        public Queue<Point> PointsToVisit
        {
            get { return m_PointsToVisit; }
        }

        private void MakeMove()
        {
            if (this.PointsToVisit.Count > 0)
                this.MoveTo(PointsToVisit.Dequeue());
            else
                this.ForceAutoDestruction();
        }
        public void DoActions()
        {
            switch (this.WhatToDoNext)
            {
                case WhatToDoNextAction.WaitingForPoints:
                    break;
                case WhatToDoNextAction.MoveToPoint:
                    MakeMove();
                    break;
            }
        }
    }
}
