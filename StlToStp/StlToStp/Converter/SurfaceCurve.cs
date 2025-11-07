using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.StlToStp.Converter
{
    public class SurfaceCurve : Entity
{
    private Line Line { get; set; }
    
    public SurfaceCurve(List<Entity> entityList) : base(entityList)
    {
        Line = null;
    }
    public SurfaceCurve(List<Entity> entityList, Line surfaceCurveIn) : base(entityList)
    {
        Line = surfaceCurveIn;
    }
    
    public override void Serialize(StreamWriter writer)
    {
        writer.WriteLine($"#{Id} = SURFACE_CURVE('{Label}', #{Line.Id});");
    }
    
    public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
    {
        var lineId = ExtractLineId(args);
        if (lineId.HasValue)
        {
            Line = entityMap.TryGetValue(lineId.Value, out var entity) ? entity as Line : null;
        }
    }
    
    private static int? ExtractLineId(string args)
    {
        var commaIndex = args.IndexOf(',');
        if (commaIndex == -1) return null;
        
        string normalizedArgs = args.Substring(commaIndex + 1)
            .Replace(",", " ")
            .Replace("#", " ");
        
        var parts = normalizedArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 1) return null;
        
        return int.TryParse(parts[0], out var lineId) ? lineId : null;
    }
}
}