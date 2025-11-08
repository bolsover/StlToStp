using System.Collections.Generic;
using System.IO;


namespace Bolsover.Converter
{
    public abstract class Entity
    {
        public int Id { get;  }
        public string Label { get;  }

        protected Entity(List<Entity> entityList)
        {
            entityList.Add(this);
            Id = entityList.Count; // IDs start at 1
        }


        // Abstract methods
        public abstract void Serialize(StreamWriter writer);
    }
}