using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.StlToStp.Converter
{
    public class Line : Entity
{
    private Point Point { get; set; }
    private Vector Vector { get; set; }

    public Line(List<Entity> entityList) : base(entityList)
    {
    }

    public Line(List<Entity> entityList, Point pointIn, Vector vectorIn) : base(entityList)
    {
        Point = pointIn;
        Vector = vectorIn;
    }

    public override void Serialize(StreamWriter writer)
    {
        writer.WriteLine($"#{Id} = LINE('{Label}', #{Point.Id}, #{Vector.Id});");
    }

    public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
    {
        var start = args.IndexOf(',');
        if (start == -1) return;

        string argStr = ParseArgumentString(args, start);
        var parts = argStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2) return;
        var pointId = ParseEntityId(parts[0]);
        var vectorId = ParseEntityId(parts[1]);

        Point = ResolveEntity<Point>(entityMap, pointId);
        Vector = ResolveEntity<Vector>(entityMap, vectorId);
    }

    private static string ParseArgumentString(string args, int start)
    {
        return args.Substring(start + 1)
            .Replace(",", " ")
            .Replace("#", " ");
    }

    private static int ParseEntityId(string part)
    {
        return int.TryParse(part, out var id) ? id : 0;
    }

    private static T ResolveEntity<T>(Dictionary<int, Entity> entityMap, int entityId) where T : Entity
    {
        return entityMap.TryGetValue(entityId, out var entity) ? entity as T : null;
    }
}
}