using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class Point : Entity
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        // Constructors
        public Point(List<Entity> entityList) : base(entityList)
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

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

        // ParseArgs using LINQ
        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var coordinates = ExtractCoordinates(args);
            if (coordinates != null)
            {
                X = coordinates.Value.x;
                Y = coordinates.Value.y;
                Z = coordinates.Value.z;
            }
        }

        private static (double x, double y, double z)? ExtractCoordinates(string args)
        {
            var start = args.IndexOf('(');
            var end = args.LastIndexOf(')');
            if (start == -1 || end == -1 || end <= start) return null;

            var argStr = args.Substring(start + 1, end - start - 1);
            var values = argStr
                .Replace(",", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => double.TryParse(s, out var val) ? val : 0.0)
                .ToList();

            if (values.Count < 3) return null;
            return (values[0], values[1], values[2]);
        }
    }
}