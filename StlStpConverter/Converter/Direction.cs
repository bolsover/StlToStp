using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.Converter
{
    public class Direction : Entity
    {
        private double X { get; }
        private double Y { get; }
        private double Z { get; }


        public Direction(List<Entity> entityList, double xIn, double yIn, double zIn) : base(entityList)
        {
            X = xIn;
            Y = yIn;
            Z = zIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter stream)
        {
            stream.WriteLine($"#{Id} = DIRECTION('{Label}', ({X}, {Y}, {Z}));");
        }
    }
}