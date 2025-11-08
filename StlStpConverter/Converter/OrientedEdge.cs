using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.Converter
{
    public class OrientedEdge : Entity
    {
        private const int EdgeIdIndex = 2;
        private const int DirectionIndex = 3;
        private const int MinimumArgumentCount = 4;
        private const string TrueValue = ".T.";

        private bool IsForward { get; set; }
        private EdgeCurve Edge { get; set; }

        // Constructors
        public OrientedEdge(List<Entity> entityList) : base(entityList)
        {
            Edge = null;
            IsForward = false;
        }

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

        // ParseArgs method
        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var argumentParts = ExtractArgumentString(args);
            if (argumentParts == null || argumentParts.Length < MinimumArgumentCount) return;

            ParseEdgeIdAndDirection(entityMap, argumentParts);
        }

        private static string[] ExtractArgumentString(string args)
        {
            var start = args.IndexOf(',');
            if (start == -1) return null;

            var argStr = args.Substring(start + 1)
                .Replace(",", " ")
                .Replace("#", " ");

            return argStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void ParseEdgeIdAndDirection(Dictionary<int, Entity> entityMap, string[] parts)
        {
            var edgeId = int.TryParse(parts[EdgeIdIndex], out var val) ? val : 0;
            Edge = entityMap.TryGetValue(edgeId, out var value) ? value as EdgeCurve : null;
            IsForward = parts[DirectionIndex] == TrueValue;
        }
    }
}