using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bolsover.Converter
{
    public abstract class StlReader
    {
        public static async Task<List<double>> ReadStlAsciiAsync(string fileName)
        {
            var nodes = new List<double>();

            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($@"File not found: {fileName}");
                    return nodes;
                }

                using var reader = new StreamReader(fileName);
                while (await reader.ReadLineAsync() is { } line)
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 4 || parts[0] != "vertex") continue;
                    if (!double.TryParse(parts[1], out var x) ||
                        !double.TryParse(parts[2], out var y) ||
                        !double.TryParse(parts[3], out var z)) continue;
                    nodes.Add(x);
                    nodes.Add(y);
                    nodes.Add(z);
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($@"I/O error while reading file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($@"Access denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Unexpected error: {ex.Message}");
            }

            return nodes;
        }


        public static async Task<List<double>> ReadStlBinaryAsync(string fileName)
        {
            var nodes = new List<double>();

            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($@"Failed to open binary STL file: {fileName}");
                    return nodes;
                }

                using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: 4096, useAsync: true);
                using var br = new BinaryReader(fs);
                // Read an 80-byte header
                br.ReadBytes(80);

                // Read the number of triangles (uint32)
                var tris = br.ReadUInt32();

                // Pre-allocate list for performance
                nodes.Capacity = (int)tris * 9;

                // Buffer for one triangle (normal + 3 vertices + attribute)
                var buffer = new byte[(3 + 9) * sizeof(float) + sizeof(ushort)];

                for (var i = 0; i < tris; i++)
                {
                    var bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead != buffer.Length)
                        throw new EndOfStreamException("Unexpected end of STL file.");

                    // Skip normal (first 3 floats)
                    var offset = 3 * sizeof(float);

                    // Read 9 floats for vertices
                    for (var j = 0; j < 9; j++)
                    {
                        var value = BitConverter.ToSingle(buffer, offset);
                        nodes.Add(value);
                        offset += sizeof(float);
                    }
                    // Skip attribute (last 2 bytes)
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($@"I/O error while reading file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($@"Access denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Unexpected error: {ex.Message}");
            }

            return nodes;
        }


        public static async Task<List<double>> ReadStlAsync(string fileName)
        {
            var nodes = new List<double>();

            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($@"Failed to open STL file: {fileName}");
                    return nodes;
                }

                var fileInfo = new FileInfo(fileName);
                var fileSize = fileInfo.Length;

                // The minimum size of an empty ASCII STL file is 15 bytes
                if (fileSize < 15)
                {
                    Console.WriteLine($@"Invalid STL file: {fileName}");
                    return nodes;
                }

                // Read the first 5 bytes to check for "solid"
                var firstBytes = new byte[5];
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read,
                           bufferSize: 4096, useAsync: true))
                {
                    var bytesRead = await fs.ReadAsync(firstBytes, 0, 5);
                    if (bytesRead < 5)
                    {
                        Console.WriteLine($@"Invalid STL file: {fileName}");
                        return nodes;
                    }
                }

                var firstWord = Encoding.ASCII.GetString(firstBytes);

                if (firstWord == "solid")
                {
                    // Possible ASCII STL, but could be binary with "solid" in the header
                    // Check further: read more content and look for "facet" keyword
                    var looksAscii = await LooksLikeAsciiAsync(fileName);

                    if (looksAscii)
                    {
                        nodes = await ReadStlAsciiAsync(fileName);
                    }
                    else
                    {
                        nodes = await ReadStlBinaryAsync(fileName);
                    }
                }
                else
                {
                    nodes = await ReadStlBinaryAsync(fileName);
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($@"I/O error: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($@"Access denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Unexpected error: {ex.Message}");
            }

            return nodes;
        }

        private static async Task<bool> LooksLikeAsciiAsync(string fileName)
        {
            // Heuristic: ASCII STL should contain "facet" somewhere in the first few KB
            const int checkSize = 1024; // 1 KB
            var buffer = new byte[checkSize];

            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096,
                useAsync: true);
            var bytesRead = await fs.ReadAsync(buffer, 0, checkSize);
            var content = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            return content.Contains("facet");
        }


        public static async Task<int> Convert(string inputFile, string outputFile, double tol = 1e-6)
        {
            // Read STL file (async)
            var nodes = await ReadStlAsync(inputFile);
            if (nodes.Count / 9 == 0)
            {
                Console.WriteLine($@"No triangles found in STL file: {inputFile}");
                return 1;
            }

            Console.WriteLine($@"Read {nodes.Count / 9} triangles from {inputFile}");

            // Build STEP body and write output
            var stepWriter = new StepWriter();
            int mergedEdgeCount = 0;
            stepWriter.BuildTriBody(nodes, tol, ref mergedEdgeCount);
            stepWriter.WriteStep(outputFile);

            Console.WriteLine($@"Merged {mergedEdgeCount} edges");
            Console.WriteLine($@"Exported STEP file: {outputFile}");

            return 0;
        }
    }
}