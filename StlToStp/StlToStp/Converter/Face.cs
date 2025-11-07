using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class Face : Entity
    {
        private List<FaceBound> FaceBounds { get; set; } = new();
        private bool Dir { get; set; }
        private Plane Plane { get; set; }

        public Face(List<Entity> entityList) : base(entityList)
        {
            Dir = true;
            Plane = null;
        }

        public Face(List<Entity> entityList, List<FaceBound> faceBoundsIn, Plane planeIn, bool dirIn) : base(entityList)
        {
            FaceBounds = faceBoundsIn;
            Dir = dirIn;
            Plane = planeIn;
        }

        public override void Serialize(StreamWriter writer)
        {
            writer.Write($"#{Id} = ADVANCED_FACE('{Label}', (");
            for (var i = 0; i < FaceBounds.Count; i++)
            {
                writer.Write($"#{FaceBounds[i].Id}");
                if (i != FaceBounds.Count - 1)
                    writer.Write(",");
            }

            writer.WriteLine($"),#{Plane.Id},{(Dir ? ".T." : ".F.")});");
        }

        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var startParenIndex = args.IndexOf('(');
            var endParenIndex = args.LastIndexOf(')');

            if (startParenIndex == -1 || endParenIndex == -1 || endParenIndex <= startParenIndex)
                return;

            ExtractFaceBounds(entityMap, args, startParenIndex, endParenIndex);
            ExtractPlaneAndDirection(entityMap, args, endParenIndex);
        }

        private void ExtractFaceBounds(Dictionary<int, Entity> entityMap, string args, int startParenIndex,
            int endParenIndex)
        {
            var boundsSection = args.Substring(startParenIndex + 1, endParenIndex - startParenIndex - 1)
                .Replace("#", " ");

            var boundIds = boundsSection
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0);

            foreach (var id in boundIds)
            {
                if (entityMap.TryGetValue(id, out var entity) && entity is FaceBound faceBound)
                {
                    FaceBounds.Add(faceBound);
                }
            }
        }

        private void ExtractPlaneAndDirection(Dictionary<int, Entity> entityMap, string args, int endParenIndex)
        {
            var remainingArgs = args.Substring(endParenIndex + 1)
                .Replace("#", " ")
                .Replace(",", " ");

            var parts = remainingArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[0], out var planeId) && entityMap.TryGetValue(planeId, out var entity))
                {
                    Plane = entity as Plane;
                }

                Dir = parts[1] == ".T.";
            }
        }
    }
}