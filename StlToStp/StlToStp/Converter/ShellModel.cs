using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class ShellModel : Entity
    {
        private List<Shell> Shells { get; } = new();

        public ShellModel(List<Entity> entityList) : base(entityList)
        {
        }

        public ShellModel(List<Entity> entityList, List<Shell> shellsIn) : base(entityList)
        {
            Shells = shellsIn;
        }

        public override void Serialize(StreamWriter writer)
        {
            var shellIds = SerializeShellIds();
            writer.WriteLine($"#{Id} = SHELL_BASED_SURFACE_MODEL('{Label}', ({shellIds}));");
        }

        private string SerializeShellIds()
        {
            return string.Join(",", Shells.Select(shell => $"#{shell.Id}"));
        }

        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var ids = ExtractEntityIds(args);

            foreach (var id in ids)
            {
                if (entityMap.TryGetValue(id, out var entity) && entity is Shell shell)
                {
                    Shells.Add(shell);
                }
            }
        }

        private static List<int> ExtractEntityIds(string args)
        {
            var start = args.IndexOf('(');
            var end = args.LastIndexOf(')');

            if (start == -1 || end == -1 || end <= start)
                return new List<int>();

            var argStr = args.Substring(start + 1, end - start - 1).Replace("#", " ");

            return argStr
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var val) ? val : 0)
                .Where(id => id != 0)
                .ToList();
        }
    }
}