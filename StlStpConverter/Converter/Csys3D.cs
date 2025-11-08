using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Bolsover.Converter
{
    public class Csys3D : Entity
    {
        private Direction Direction1 { get; }
        private Direction Direction2 { get; }
        private Point Point { get; }


        public Csys3D(List<Entity> entityList, Direction direction1, Direction direction2, Point point) : base(
            entityList)
        {
            Direction1 = direction1;
            Direction2 = direction2;
            Point = point;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = AXIS2_PLACEMENT_3D('{Label}',#{Point.Id},#{Direction1.Id},#{Direction2.Id});");
        }
    }
}