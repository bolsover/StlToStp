using System.Collections.Generic;
using System.IO;

namespace Bolsover.Converter
{
    public class Vertex : Entity
    {
        public Point Point { get; }


        public Vertex(List<Entity> entityList, Point pointIn) : base(entityList)
        {
            Point = pointIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = VERTEX_POINT('{Label}', #{Point.Id});");
        }
    }
}