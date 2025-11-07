using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class EdgeLoop : Entity
{
    private List<OrientedEdge> OrientedEdges { get; set; } = new List<OrientedEdge>();
    
    // Constructors
    public EdgeLoop(List<Entity> entityList) : base(entityList)
    {
    }
    
    public EdgeLoop(List<Entity> entityList, List<OrientedEdge> edgesIn) : base(entityList)
    {
        OrientedEdges = edgesIn;
    }
    
    // Serialize method
    public override void Serialize(StreamWriter writer)
    {
        writer.Write($"#{Id} = EDGE_LOOP('{Label}', (");
        writer.Write(SerializeEdgeReferences());
        writer.WriteLine("));");
    }
    
    private string SerializeEdgeReferences()
    {
        return string.Join(",", OrientedEdges.Select(edge => $"#{edge.Id}"));
    }
    
    // ParseArgs using LINQ
    public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
    {
        var ids = ExtractEntityIds(args);
        
        foreach (var id in ids)
        {
            if (entityMap.ContainsKey(id) && entityMap[id] is OrientedEdge edge)
            {
                OrientedEdges.Add(edge);
            }
        }
    }
    
    private static List<int> ExtractEntityIds(string args)
    {
        var start = args.IndexOf('(');
        var end = args.LastIndexOf(')');
        
        if (start == -1 || end == -1 || end <= start)
            return new List<int>();
        
        var argStr = args.Substring(start + 1, end - start - 1)
            .Replace("#", " ");
        
        return argStr
            .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var val) ? val : 0)
            .ToList();
    }
}
}