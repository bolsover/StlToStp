using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bolsover.Converter;

namespace Bolsover.Converter
{
    public class Plane : Entity
    {
        private Csys3D Csys { get; }

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
    }
}