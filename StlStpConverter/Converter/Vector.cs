using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.Converter
{
    public class Vector : Entity
    {
        private double Length { get; set; }
        private Direction Dir { get; set; }

        // Constructors
        public Vector(List<Entity> entityList) : base(entityList)
        {
            Dir = null;
            Length = 0;
        }

        public Vector(List<Entity> entityList, Direction dirIn, double lenIn) : base(entityList)
        {
            Dir = dirIn;
            Length = lenIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = VECTOR('{Label}', #{Dir.Id}, {Length});");
        }

        // ParseArgs method
        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var start = args.IndexOf(',');
            if (start == -1) return;

            string argStr = ParseArgumentString(args, start);
            var parts = argStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2) return;

            var dirId = ParseEntityId(parts[0]);
            Length = ParseDouble(parts[1]);
            Dir = ResolveEntity<Direction>(entityMap, dirId);
        }

        private static string ParseArgumentString(string args, int start)
        {
            return args.Substring(start + 1)
                .Replace(",", " ")
                .Replace("#", " ");
        }

        private static int ParseEntityId(string part)
        {
            return int.TryParse(part, out var val) ? val : 0;
        }

        private static double ParseDouble(string part)
        {
            return double.TryParse(part, out var val) ? val : 0;
        }

        private static T ResolveEntity<T>(Dictionary<int, Entity> entityMap, int entityId) where T : Entity
        {
            return entityMap.TryGetValue(entityId, out var value) ? value as T : null;
        }
    }
}