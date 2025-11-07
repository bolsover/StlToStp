using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.StlToStp.Converter
{
    public class Vertex : Entity
    {
        public Point Point { get; private set; }

        // Constructors
        public Vertex(List<Entity> entityList) : base(entityList)
        {
            Point = null;
        }

        public Vertex(List<Entity> entityList, Point pointIn) : base(entityList)
        {
            Point = pointIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = VERTEX_POINT('{Label}', #{Point.Id});");
        }

        // ParseArgs method
        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            int pointId = ExtractPointIdFromArgs(args);
            if (pointId > 0)
            {
                Point = entityMap.TryGetValue(pointId, out var value) ? value as Point : null;
            }
        }

        private int ExtractPointIdFromArgs(string args)
        {
            var start = args.IndexOf(',');
            if (start == -1) return 0;

            var argStr = args.Substring(start + 1)
                .Replace(",", " ")
                .Replace("#", " ");

            var parts = argStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1) return 0;

            return int.TryParse(parts[0], out var val) ? val : 0;
        }
    }
}