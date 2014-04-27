using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using PH.Common;
using PH.Map;

namespace Tutorials
{

    class myPlayer: PH.Common.Player
    {
        // enum WhatToDoNextAction
        public enum AI_WhatToDoNextAction
        {
            BuildExplorer = 0,
            FillNavPoints = 1,
            BuildCollector = 2,
            MoveToHoshimiPoint = 3,
            BuildNeedle = 4,
            NothingToDo = 5,
        }

        // private variables
        private AI_WhatToDoNextAction m_WhatToDoNext = AI_WhatToDoNextAction.BuildExplorer;
        private List<Entity> m_AznEntities = new List<Entity>();
        private List<Entity> m_HoshimiEntities = new List<Entity>();
        private List<Point> m_NavigationPoints = new List<Point>();
        private List<Point> m_NeedlePoints = new List<Point>();
        private List<Point> m_EmptyNeedlePoints = new List<Point>();
        private List<Point> m_FullNeedlePoints = new List<Point>();

        private int CollectorBuilded;

        // public variables
        public List<Entity> AznEntities { get { return m_AznEntities; } }
        public List<Entity> HoshimiEntities { get { return m_HoshimiEntities; } }
        public List<Point> NavigationPoints { get { return m_NavigationPoints; } }
        public List<Point> NeedlePoints { get { return m_NeedlePoints; } }
        public List<Point> EmptyNeedlePoints { get { return m_EmptyNeedlePoints; } }
        public List<Point> FullNeedlePoints { get { return m_FullNeedlePoints; } }

        public AI_WhatToDoNextAction AI_WhatToDoNext
        {
            get { return m_WhatToDoNext; }
            set { m_WhatToDoNext = value; }
        }

        public myPlayer() { }
        public myPlayer(string name, int ID) : base(name, ID)
        {
            this.ChooseInjectionPoint += new PH.Common.ChooseInjectionPointHandler(myPlayer_ChooseInjectionPoint);
            this.WhatToDoNext += new PH.Common.WhatToDoNextHandler(myPlayer_WhatToDoNext);
        }

        private void myPlayer_ChooseInjectionPoint()
        {
            foreach(Entity ent in this.Tissue.Entities)
            {
                switch (ent.EntityType)
                {
                    case EntityEnum.AZN:
                        m_AznEntities.Add(ent);
                        break;
                    case EntityEnum.HoshimiPoint:
                        m_HoshimiEntities.Add(ent);
                        break;
                }
            }

            foreach (PH.Mission.BaseObjective obj in this.Mission.Objectives)
            {
                if (obj is PH.Mission.NavigationObjective && obj.Bonus > 0)
                {
                    PH.Mission.NavigationObjective navObj = (PH.Mission.NavigationObjective)obj;
                    foreach (PH.Mission.NavPoint np in navObj.NavPoints)
                        NavigationPoints.Add(np.Location);
                }
                else if (obj is PH.Mission.UniqueNavigationObjective && obj.Bonus > 0)
                {
                    PH.Mission.UniqueNavigationObjective uniqueNavObj = (PH.Mission.UniqueNavigationObjective)obj;
                    foreach (PH.Mission.NavPoint np in uniqueNavObj.NavPoints)
                        NavigationPoints.Add(np.Location);
                }
            }

            Point NavigationMiddle = Utils.getMiddlePoint(NavigationPoints.ToArray());
            this.InjectionPointWanted = Utils.getValidPoint(this.Tissue, NavigationMiddle);
        }

        private void UpdateInformations()
        {
            NeedlePoints.Clear();
            EmptyNeedlePoints.Clear();
            FullNeedlePoints.Clear();

            CollectorBuilded = 0;
            foreach (NanoBot bot in this.NanoBots)
            {
                if (bot is Needle)
                {
                    NeedlePoints.Add(bot.Location);
                    if (bot.Stock == 100)
                        FullNeedlePoints.Add(bot.Location);
                    else
                        EmptyNeedlePoints.Add(bot.Location);
                }
                else if (bot is Collector)
                    CollectorBuilded++; 
            }
        }

        private void AI_DoActions()
        {
            if (this.AI.State == NanoBotState.WaitingOrders)
            {
                switch (this.AI_WhatToDoNext)
                {
                    case AI_WhatToDoNextAction.BuildExplorer:
                        if (this.AI.Build(typeof(Explorer)))
                            this.AI_WhatToDoNext = AI_WhatToDoNextAction.FillNavPoints;
                            break;
                    case AI_WhatToDoNextAction.FillNavPoints:
                        foreach (NanoBot bot in this.NanoBots)
                        {
                            if (bot is Explorer &&
                                ((Explorer)bot).WhatToDoNext == Explorer.WhatToDoNextAction.WaitingForPoints)
                            {
                                SelectObjectivePoints((Explorer)bot);
                                this.AI_WhatToDoNext = AI_WhatToDoNextAction.NothingToDo;
                                break;
                            }
                        }
                        break;
                    case AI_WhatToDoNextAction.BuildCollector:
                        if (this.AI.Build(typeof(Collector)))
                            if (CollectorBuilded >= Collector.SquadNumber - 1)
                                this.AI_WhatToDoNext = AI_WhatToDoNextAction.MoveToHoshimiPoint;
                        break;
                    case AI_WhatToDoNextAction.NothingToDo:
                        break;
                }
            }
        }

        private void SelectObjectivePoints(Explorer explo)
        {
            explo.PointsToVisit.Clear();
            explo.PointsToVisit.Enqueue(new Point(117, 142));
            explo.PointsToVisit.Enqueue(new Point(111, 154));
            explo.PointsToVisit.Enqueue(new Point(128, 195));
            explo.WhatToDoNext = Explorer.WhatToDoNextAction.MoveToPoint;
        }

        private void myPlayer_WhatToDoNext()
        {
            UpdateInformations();
            AI_DoActions();

            foreach (NanoBot bot in this.NanoBots)
                if (bot is IActionable && bot.State == NanoBotState.WaitingOrders)
                    ((IActionable)bot).DoActions();
        }

        public override System.Drawing.Bitmap Flag
        {
            get { return Properties.Resources.rcFlag; }
        }

    }
}
