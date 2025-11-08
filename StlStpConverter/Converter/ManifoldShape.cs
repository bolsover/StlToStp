using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bolsover.Converter;

namespace Bolsover.Converter
{
    public class ManifoldShape : Entity
    {
        private Csys3D Csys { get; }
        private ShellModel ShellModel { get; }


        public ManifoldShape(List<Entity> entityList, Csys3D csysIn, ShellModel shellModelIn) : base(entityList)
        {
            Csys = csysIn;
            ShellModel = shellModelIn;
        }

        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine(
                $"#{Id} = MANIFOLD_SURFACE_SHAPE_REPRESENTATION('{Label}', (#{Csys.Id}, #{ShellModel.Id}));");
        }
    }
}