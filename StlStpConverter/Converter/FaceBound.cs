using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.Converter
{
    public class FaceBound : Entity
    {
        private EdgeLoop EdgeLoop { get; }
        private bool Orientation { get; }


        public FaceBound(List<Entity> entityList, EdgeLoop edgeLoopIn, bool orientationIn) : base(entityList)
        {
            EdgeLoop = edgeLoopIn;
            Orientation = orientationIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = FACE_BOUND('{Label}', #{EdgeLoop.Id},{(Orientation ? ".T." : ".F.")});");
        }
    }
}