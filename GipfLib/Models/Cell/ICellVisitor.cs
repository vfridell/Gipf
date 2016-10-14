using System;

namespace GipfLib.Models
{
    public interface ICellVisitor
    {
        void VisitCellHandler(object sender, VisitCellEventArgs e);
        void VisitWallHandler(object sender, VisitWallEventArgs e);

        void PostProcessHandler(object sender, EventArgs e);
        void PreProcessHandler(object sender, EventArgs e);
    }

    public class VisitCellEventArgs : EventArgs
    {
        public readonly Cell cell;
        public readonly bool newLine;

        public VisitCellEventArgs(Cell cellP, bool NewLine)
        {
            cell = cellP;
            newLine = NewLine;
        }
    }

    public class VisitWallEventArgs : EventArgs
    {
        public readonly Wall wall;

        public VisitWallEventArgs(Wall wallP)
        {
            wall = wallP;
        }
    }
}
