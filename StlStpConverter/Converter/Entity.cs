using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.Converter
{
    public abstract class Entity
    {
        public int Id { get; set; }
        public string Label { get; set; }

        protected Entity(List<Entity> entityList)
        {
            entityList.Add(this);
            Id = entityList.Count; // IDs start at 1
        }
 

        // Abstract methods
        public abstract void Serialize(StreamWriter writer);
        public abstract void ParseArgs(Dictionary<int, Entity> entityMap, string args);
    }
}