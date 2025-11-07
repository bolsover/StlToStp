using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class Csys3D : Entity
    {
        private Direction Dir1 { get; set; }
        private Direction Dir2 { get; set; }
        private Point Point { get; set; }

        // Constructors
        public Csys3D(List<Entity> entityList) : base(entityList)
        {
        }

        public Csys3D(List<Entity> entityList, Direction dir1In, Direction dir2In, Point pointIn) : base(entityList)
        {
            Dir1 = dir1In;
            Dir2 = dir2In;
            Point = pointIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = AXIS2_PLACEMENT_3D('{Label}',#{Point.Id},#{Dir1.Id},#{Dir2.Id});");
        }

        // ParseArgs using LINQ
        public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
        {
            var ids = ParseEntityIds(args);
            if (ids.Count >= 3)
            {
                Point = TryGetEntityById<Point>(entityMap, ids[0]);
                Dir1 = TryGetEntityById<Direction>(entityMap, ids[1]);
                Dir2 = TryGetEntityById<Direction>(entityMap, ids[2]);
            }
        }

        private List<int> ParseEntityIds(string args)
        {
            int start = args.IndexOf(',');
            if (start == -1) return new List<int>();

            string argStr = args.Substring(start + 1)
                .Replace(",", " ")
                .Replace("#", " ");

            return argStr
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var val) ? val : 0)
                .ToList();
        }

        private T TryGetEntityById<T>(Dictionary<int, Entity> entityMap, int id) where T : Entity
        {
            return entityMap.TryGetValue(id, out var entity) ? entity as T : null;
        }
    }
}