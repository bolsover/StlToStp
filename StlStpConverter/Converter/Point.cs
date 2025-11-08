using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.Converter
{
    public class Point : Entity
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }


        public Point(List<Entity> entityList, double xIn, double yIn, double zIn) : base(entityList)
        {
            X = xIn;
            Y = yIn;
            Z = zIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = CARTESIAN_POINT('{Label}', ({X},{Y},{Z}));");
        }
    }
}