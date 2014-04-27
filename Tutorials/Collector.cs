using System;
using System.Collections.Generic;
using System.Text;

namespace Tutorials
{
    [PH.Common.Characteristics(
        ContainerCapacity = 20,
        CollectTransfertSpeed = 5,
        Scan = 5,
        MaxDamage = 0,
        DefenseDistance = 0,
        Constitution = 20)
    ]
    class Collector: PH.Common.NanoCollector, IActionable
    {
        public enum WhatToDoNextAction
        {
            MoveToAZN = 0,
            CollectAZN = 1,
            MoveToHoshimi = 2,
            TransferToNeedle = 3,
        }


        private WhatToDoNextAction m_WhatToDoNextAction = WhatToDoNextAction.CollectAZN;
        public WhatToDoNextAction WhatToDoNext
        {
            get { return m_WhatToDoNextAction; }
            set { m_WhatToDoNextAction = value; }
        }
        public const int SquadNumber = 0;
        public void DoActions()
        {
            switch(this.WhatToDoNext)
            {
                case WhatToDoNextAction.CollectAZN:
                    this.CollectFrom(this.Location, 4);
                    this.WhatToDoNext = WhatToDoNextAction.MoveToHoshimi;
                    break;
                case WhatToDoNextAction.MoveToHoshimi:
                    this.MoveTo(Utils.getNearestPoint(this.Location, ((myPlayer)this.PlayerOwner).HoshimiEntities));
                    this.WhatToDoNext = WhatToDoNextAction.TransferToNeedle;
                    break;
                case WhatToDoNextAction.TransferToNeedle:
                    this.TransferTo(this.Location, 4);
                    this.WhatToDoNext = WhatToDoNextAction.CollectAZN;
                    break;
                case WhatToDoNextAction.MoveToAZN:
                    this.MoveTo(Utils.getNearestPoint(this.Location, ((myPlayer)this.PlayerOwner).AznEntities));
                    this.WhatToDoNext = WhatToDoNextAction.CollectAZN;
                    break;
            }
        }
    }
}
