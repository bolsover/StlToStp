using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class Plane : Entity
{
    private Csys3D Csys { get; set; }
    
    // Constructor with optional parameter
    public Plane(List<Entity> entityList, Csys3D csysIn = null) : base(entityList)
    {
        Csys = csysIn;
    }
    
    // Serialize method
    public override void Serialize(StreamWriter writer)
    {
        if (Csys == null)
            throw new InvalidOperationException("Cannot serialize Plane without a valid Csys.");
            
        writer.WriteLine($"#{Id} = PLANE('{Label}',#{Csys.Id});");
    }
    
    // ParseArgs using LINQ
    public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
    {
        var csysId = ExtractCsysIdFromArgs(args);
        if (csysId.HasValue)
        {
            Csys = GetEntityById<Csys3D>(entityMap, csysId.Value);
        }
    }
    
    private int? ExtractCsysIdFromArgs(string args)
    {
        var start = args.IndexOf(',');
        if (start == -1) return null;
        
        var argStr = args.Substring(start + 1)
            .Replace(",", " ")
            .Replace("#", " ");
        
        var ids = argStr
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var val) ? val : 0)
            .Where(id => id > 0)
            .ToList();
        
        return ids.Count > 0 ? ids[0] : (int?)null;
    }
    
    private T GetEntityById<T>(Dictionary<int, Entity> entityMap, int id) where T : Entity
    {
        return entityMap.TryGetValue(id, out var entity) ? entity as T : null;
    }
}
}