using System;
using System.Collections.Generic;
using System.IO;

namespace Bolsover.Converter
{
    public class Vector : Entity
    {
        private double Length { get; }
        private Direction Dir { get; }


        public Vector(List<Entity> entityList, Direction dirIn, double lenIn) : base(entityList)
        {
            Dir = dirIn;
            Length = lenIn;
        }

        // Serialize method
        public override void Serialize(StreamWriter writer)
        {
            writer.WriteLine($"#{Id} = VECTOR('{Label}', #{Dir.Id}, {Length});");
        }
    }
}