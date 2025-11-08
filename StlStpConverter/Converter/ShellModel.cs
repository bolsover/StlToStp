using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.Converter
{
    public class ShellModel : Entity
    {
        private List<Shell> Shells { get; }


        public ShellModel(List<Entity> entityList, List<Shell> shellsIn) : base(entityList)
        {
            Shells = shellsIn;
        }

        public override void Serialize(StreamWriter writer)
        {
            var shellIds = SerializeShellIds();
            writer.WriteLine($"#{Id} = SHELL_BASED_SURFACE_MODEL('{Label}', ({shellIds}));");
        }

        private string SerializeShellIds()
        {
            return string.Join(",", Shells.Select(shell => $"#{shell.Id}"));
        }
    }
}