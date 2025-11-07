using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class Shell : Entity
    {
        private List<Face> Faces { get; } = new();
        private bool IsOpen { get; set; }

        public Shell(List<Entity> entityList) : base(entityList)
        {
            IsOpen = true;
        }

        public Shell(List<Entity> entityList, List<Face> faces) : base(entityList)
        {
            Faces = faces;
            IsOpen = true;
        }

        public override void Serialize(StreamWriter writer)
        {
            writer.Write($"#{Id} = {(IsOpen ? "OPEN_SHELL" : "CLOSED_SHELL")}('{Label}',(");
            WriteFaceReferences(writer);
            writer.WriteLine("));");
        }

        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var ids = ExtractEntityIds(args);
            foreach (var id in ids)
            {
                if (entityMap.TryGetValue(id, out var entity) && entity is Face face)
                {
                    Faces.Add(face);
                }
            }
        }

        private void WriteFaceReferences(StreamWriter writer)
        {
            writer.Write(string.Join(",", Faces.Select(f => $"#{f.Id}")));
        }

        private static List<int> ExtractEntityIds(string args)
        {
            var start = args.IndexOf('(');
            var end = args.LastIndexOf(')');
            if (start == -1 || end == -1 || end <= start) return new List<int>();

            var argStr = args.Substring(start + 1, end - start - 1).Replace("#", " ");
            return argStr
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var val) ? val : 0)
                .Where(id => id != 0)
                .ToList();
        }
    }
}