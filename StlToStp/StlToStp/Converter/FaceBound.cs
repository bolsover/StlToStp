using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class FaceBound : Entity
    {
        private EdgeLoop EdgeLoop { get; set; }
        private bool Orientation { get; set; }

        // Constructors
        public FaceBound(List<Entity> entityList) : base(entityList)
        {
            EdgeLoop = null;
            Orientation = true;
        }

        public FaceBound(List<Entity> entityList, EdgeLoop edgeLoopIn, bool orientationIn) : base(entityList)
        {
            EdgeLoop = edgeLoopIn;
            Orientation = orientationIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = FACE_BOUND('{Label}', #{EdgeLoop.Id},{(Orientation ? ".T." : ".F.")});");
        }

        // ParseArgs using LINQ
        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var start = args.IndexOf(',');
            if (start == -1) return;

            var parts = ExtractArgumentParts(args, start);

            if (parts.Count >= 2)
            {
                ParseEdgeLoopAndOrientation(entityMap, parts);
            }
        }

        private List<string> ExtractArgumentParts(string args, int start)
        {
            return args.Substring(start + 1)
                .Replace(",", " ")
                .Replace("#", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        private void ParseEdgeLoopAndOrientation(Dictionary<int, Entity> entityMap, List<string> parts)
        {
            if (int.TryParse(parts[0], out var edgeLoopId) && entityMap.TryGetValue(edgeLoopId, out var entity))
            {
                EdgeLoop = entity as EdgeLoop;
            }

            Orientation = parts[1] == ".T.";
        }
    }
}