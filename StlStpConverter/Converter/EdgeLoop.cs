using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.Converter
{
    public class EdgeLoop : Entity
    {
        private List<OrientedEdge> OrientedEdges { get; }


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
    }
}