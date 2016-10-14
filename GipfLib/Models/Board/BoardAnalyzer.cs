using System;
using System.Collections.Generic;

namespace GipfLib.Models
{
    public class BoardAnalyzer
    {
        IEnumerable<Cell> _cells;
        public BoardAnalyzer(IEnumerable<Cell> cells)
        {
            _cells = cells;
        }

        public void AddVisitor(ICellVisitor visitor)
        {
            VisitCell += visitor.VisitCellHandler;
            VisitWall += visitor.VisitWallHandler;
            PostProcess += visitor.PostProcessHandler;
            PreProcess += visitor.PreProcessHandler;
        }

        protected event EventHandler<VisitCellEventArgs> VisitCell;
        protected event EventHandler<VisitWallEventArgs> VisitWall;
        protected event EventHandler PostProcess;
        protected event EventHandler PreProcess;
        protected virtual void OnVisitCell(VisitCellEventArgs e)
        {
            if (VisitCell != null) VisitCell?.Invoke(this, e);
        }

        protected virtual void OnVisitWall(VisitWallEventArgs e)
        {
            if (VisitWall != null) VisitWall?.Invoke(this, e);
        }

        protected virtual void OnPostProcess(EventArgs e)
        {
            if (PostProcess != null) PostProcess?.Invoke(this, e);
        }

        protected virtual void OnPreProcess(EventArgs e)
        {
            if (PreProcess != null) PreProcess?.Invoke(this, e);
        }

        public void AnalyzeBoard()
        {
            bool newLine = true;
            OnPreProcess(EventArgs.Empty);
            foreach (Cell cell in _cells)
            {
                if (cell is Wall)
                {
                    OnVisitWall(new VisitWallEventArgs((Wall)cell));
                    newLine = true;
                }
                else
                {
                    OnVisitCell(new VisitCellEventArgs(cell, newLine));
                    newLine = false;
                }
            }
            OnPostProcess(EventArgs.Empty);
        }
    }

}
