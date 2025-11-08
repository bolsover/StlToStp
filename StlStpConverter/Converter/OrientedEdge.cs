using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.Converter
{
    public class OrientedEdge : Entity
    {
        private bool IsForward { get; }
        private EdgeCurve Edge { get; }


        public OrientedEdge(List<Entity> entityList, EdgeCurve edgeCurveIn, bool dirIn) : base(entityList)
        {
            Edge = edgeCurveIn;
            IsForward = dirIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = ORIENTED_EDGE('{Label}',*,*,#{Edge.Id},{(IsForward ? ".T." : ".F.")});");
        }
    }
}