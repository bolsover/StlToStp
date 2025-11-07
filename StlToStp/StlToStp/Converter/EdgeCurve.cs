using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.StlToStp.Converter
{
    public class EdgeCurve : Entity
    {
        private Vertex Vert1 { get; set; }
        private Vertex Vert2 { get; set; }
        private SurfaceCurve SurfCurve { get; set; }
        private Line Line { get; set; }
        private bool Dir { get; set; }

        public EdgeCurve(List<Entity> entityList) : base(entityList)
        {
            Dir = true;
        }

        public EdgeCurve(List<Entity> entityList, Vertex vert1In, Vertex vert2In, SurfaceCurve surfCurveIn, bool dirIn)
            : base(entityList)
        {
            Vert1 = vert1In;
            Vert2 = vert2In;
            SurfCurve = surfCurveIn;
            Dir = dirIn;
        }

        public override void Serialize(StreamWriter writer)
        {
            var curveId = GetCurveId();
            var direction = Dir ? ".T." : ".F.";
            writer.WriteLine($"#{Id} = EDGE_CURVE('', #{Vert1.Id}, #{Vert2.Id}, #{curveId}, {direction});");
        }

        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var start = args.IndexOf(',');
            if (start == -1) return;

            var normalizedArgs = args.Substring(start + 1)
                .Replace(",", " ")
                .Replace("#", " ");

            var parts = normalizedArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) return;

            var (v1Id, v2Id, curveId) = ParseEntityIds(parts);
            ResolveEntities(entityMap, v1Id, v2Id, curveId);
            Dir = ParseDirection(parts[3]);
        }

        private int GetCurveId()
        {
            return SurfCurve?.Id ?? Line?.Id ?? 0;
        }

        private static (int v1Id, int v2Id, int curveId) ParseEntityIds(string[] parts)
        {
            var v1Id = int.TryParse(parts[0], out var val1) ? val1 : 0;
            var v2Id = int.TryParse(parts[1], out var val2) ? val2 : 0;
            var curveId = int.TryParse(parts[2], out var val3) ? val3 : 0;
            return (v1Id, v2Id, curveId);
        }

        private void ResolveEntities(Dictionary<int, Entity> entityMap, int v1Id, int v2Id, int curveId)
        {
            Vert1 = entityMap.TryGetValue(v1Id, out var value) ? value as Vertex : null;
            Vert2 = entityMap.TryGetValue(v2Id, out var value1) ? value1 as Vertex : null;

            if (!entityMap.TryGetValue(curveId, out var value2)) return;
            SurfCurve = value2 as SurfaceCurve;
            if (SurfCurve == null)
                Line = entityMap[curveId] as Line;
        }

        private static bool ParseDirection(string directionToken)
        {
            return directionToken == ".T.";
        }
    }
}