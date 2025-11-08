using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.Converter
{
    public class Shell : Entity
    {
        private List<Face> Faces { get; }
        private bool IsOpen { get; }


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


        private void WriteFaceReferences(StreamWriter writer)
        {
            writer.Write(string.Join(",", Faces.Select(f => $"#{f.Id}")));
        }
    }
}