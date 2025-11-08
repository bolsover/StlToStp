using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace Bolsover.Converter
{
    public static class FileHelpers
    {
        public static async Task WriteAllTextAsync(string path, string contents, Encoding encoding = null)
        {
            encoding ??= Encoding.ASCII;
            using var writer = new StreamWriter(path, false, encoding);
            await writer.WriteAsync(contents);
        }
    }
}