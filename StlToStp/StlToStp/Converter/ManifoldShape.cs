using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public class ManifoldShape : Entity
{
    private Csys3D Csys { get; set; }
    private ShellModel ShellModel { get; set; }

    public ManifoldShape(List<Entity> entityList) : base(entityList)
    {
    }

    public ManifoldShape(List<Entity> entityList, Csys3D csysIn, ShellModel shellModelIn) : base(entityList)
    {
        Csys = csysIn;
        ShellModel = shellModelIn;
    }

    public override void Serialize(StreamWriter writer)
    {
        writer.WriteLine($"#{Id} = MANIFOLD_SURFACE_SHAPE_REPRESENTATION('{Label}', (#{Csys.Id}, #{ShellModel.Id}));");
    }

    public override void ParseArgs(Dictionary<int, Entity> entityMap, string args)
    {
        var ids = ParseEntityIds(args);
        if (ids.Count < 2) return;

        var firstEntity = entityMap.TryGetValue(ids[0], out var entity1) ? entity1 : null;
        var secondEntity = entityMap.TryGetValue(ids[1], out var entity2) ? entity2 : null;

        AssignEntitiesByType(firstEntity, secondEntity);
    }

    private List<int> ParseEntityIds(string args)
    {
        var start = args.IndexOf('(');
        var end = args.LastIndexOf(')');
        if (start == -1 || end == -1 || end <= start) return new List<int>();

        var argStr = args.Substring(start + 1, end - start - 1)
            .Replace(",", " ")
            .Replace("#", " ");

        return argStr
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var val) ? val : 0)
            .ToList();
    }

    private void AssignEntitiesByType(Entity first, Entity second)
    {
        switch (first)
        {
            case Csys3D csys when second is ShellModel shellModel:
                Csys = csys;
                ShellModel = shellModel;
                break;
            case ShellModel model when second is Csys3D csys3D:
                ShellModel = model;
                Csys = csys3D;
                break;
        }
    }
}
}