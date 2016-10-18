using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GipfLib.Models
{
    public class GenerateMovesVisitor : ICellVisitor
    {
        List<Move> _moves = new List<Move>();
        IReadOnlyList<Move> moves => _moves.AsReadOnly();

        public void PreProcessHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void VisitCellHandler(object sender, VisitCellEventArgs e)
        {
            // do nothing
        }

        public void VisitWallHandler(object sender, VisitWallEventArgs e)
        {

            //foreach(KeyValuePair<Position, Cell> kvp in e.wall.NeighborhoodCells.Where(kvp => !(kvp.Value is Wall)))
            //{
            //    _moves.Add(new Move(kvp.Value
            //}
        }
        public void PostProcessHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
