using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class Direction : Entity
    {
        private double X { get; set; }
        private double Y { get; set; }
        private double Z { get; set; }

        // Constructors
        public Direction(List<Entity> entityList) : base(entityList)
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

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

        // ParseArgs using LINQ
        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var coordinates = ExtractCoordinatesFromArgs(args);
            if (coordinates != null)
            {
                X = coordinates.Value.x;
                Y = coordinates.Value.y;
                Z = coordinates.Value.z;
            }
        }

        private (double x, double y, double z)? ExtractCoordinatesFromArgs(string args)
        {
            var start = args.IndexOf('(');
            var end = args.LastIndexOf(')');
            if (start == -1 || end == -1 || end <= start)
                return null;

            var argStr = args.Substring(start + 1, end - start - 1);

            var values = argStr
                .Replace(",", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => double.TryParse(s, out var val) ? val : 0.0)
                .ToList();

            return values.Count >= 3
                ? (values[0], values[1], values[2])
                : null;
        }
    }
}