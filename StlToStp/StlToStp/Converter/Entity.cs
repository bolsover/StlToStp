using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
    public abstract class Entity
    {
        public int Id { get; set; }
        public string Label { get; set; }

        public Entity(List<Entity> entityList)
        {
            entityList.Add(this);
            Id = entityList.Count; // IDs start at 1
        }

        // Tokenize method
        public List<string> Tokenize(string input, string delimiters = ",")
        {
            var tokens = new List<string>();
            int lastPos = FindFirstNotOf(input, delimiters, 0);
            int pos = FindFirstOf(input, delimiters, lastPos);

            while (pos != -1 || lastPos != -1)
            {
                tokens.Add(input.Substring(lastPos, (pos == -1 ? input.Length : pos) - lastPos));
                lastPos = FindFirstNotOf(input, delimiters, pos);
                pos = FindFirstOf(input, delimiters, lastPos);
            }

            return tokens;
        }

        // Helper methods for Tokenize
        private int FindFirstNotOf(string str, string delimiters, int startIndex)
        {
            for (int i = startIndex; i < str.Length; i++)
            {
                if (!delimiters.Contains(str[i]))
                    return i;
            }

            return -1;
        }

        private int FindFirstOf(string str, string delimiters, int startIndex)
        {
            for (int i = startIndex; i < str.Length; i++)
            {
                if (delimiters.Contains(str[i]))
                    return i;
            }

            return -1;
        }

        // Abstract methods
        public abstract void Serialize(StreamWriter writer);
        public abstract void ParseArgs(Dictionary<int, Entity> entityMap, string args);
    }
}