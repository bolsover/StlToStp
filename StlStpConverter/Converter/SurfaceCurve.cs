using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.Converter
{
    public class SurfaceCurve : Entity
    {
        private Line Line { get; }


        public SurfaceCurve(List<Entity> entityList, Line surfaceCurveIn) : base(entityList)
        {
            Line = surfaceCurveIn;
        }

        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = SURFACE_CURVE('{Label}', #{Line.Id});");
        }
    }
}