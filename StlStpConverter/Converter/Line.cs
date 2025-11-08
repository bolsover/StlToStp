using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.Converter
{
    public class Line : Entity
    {
        private Point Point { get; }
        private Vector Vector { get; }


        public Line(List<Entity> entityList, Point pointIn, Vector vectorIn) : base(entityList)
        {
            Point = pointIn;
            Vector = vectorIn;
        }

        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = LINE('{Label}', #{Point.Id}, #{Vector.Id});");
        }
    }
}