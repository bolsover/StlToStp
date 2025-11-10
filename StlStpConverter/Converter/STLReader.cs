using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bolsover.Converter
{
    public abstract class StlReader
    {
        public static async Task<List<double>> ReadStlAsciiAsync(string fileName)
        {
            return await ReadStlAsciiAsync(fileName, CancellationToken.None, null);
        }

        public static async Task<List<double>> ReadStlAsciiAsync(string fileName, CancellationToken token,
            IProgress<string> progress)
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
                int lineCount = 0;
                while (await reader.ReadLineAsync() is { } line)
                {
                    token.ThrowIfCancellationRequested();
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 4 || parts[0] != "vertex") continue;
                    if (!double.TryParse(parts[1], out var x) ||
                        !double.TryParse(parts[2], out var y) ||
                        !double.TryParse(parts[3], out var z)) continue;
                    nodes.Add(x);
                    nodes.Add(y);
                    nodes.Add(z);
                    lineCount++;
                    if (lineCount % 3000 == 0)
                    {
                        progress?.Report($"Read {nodes.Count / 9} triangles so far (ASCII)...");
                    }
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
            return await ReadStlBinaryAsync(fileName, CancellationToken.None, null);
        }

        public static async Task<List<double>> ReadStlBinaryAsync(string fileName, CancellationToken token,
            IProgress<string> progress)
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
                    token.ThrowIfCancellationRequested();
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
            return await ReadStlAsync(fileName, CancellationToken.None, null);
        }

        public static async Task<List<double>> ReadStlAsync(string fileName, CancellationToken token,
            IProgress<string> progress)
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
                    var looksAscii = await LooksLikeAsciiAsync(fileName, token);

                    if (looksAscii)
                    {
                        nodes = await ReadStlAsciiAsync(fileName, token, progress);
                    }
                    else
                    {
                        nodes = await ReadStlBinaryAsync(fileName, token, progress);
                    }
                }
                else
                {
                    nodes = await ReadStlBinaryAsync(fileName, token, progress);
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
            return await LooksLikeAsciiAsync(fileName, CancellationToken.None);
        }

        private static async Task<bool> LooksLikeAsciiAsync(string fileName, CancellationToken token)
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
            return await Convert(inputFile, outputFile, tol, CancellationToken.None, null);
        }

        public static async Task<int> Convert(string inputFile, string outputFile, double tol, CancellationToken token,
            IProgress<string> progress)
        {
            progress?.Report($"Reading STL: {Path.GetFileName(inputFile)}...");
            // Read STL file (async)
            var nodes = await ReadStlAsync(inputFile, token, progress);
            token.ThrowIfCancellationRequested();
            if (nodes.Count / 9 == 0)
            {
                var msg = $"No triangles found in STL file: {inputFile}";
                progress?.Report(msg);
                Console.WriteLine(@msg);
                return 1;
            }

            var triCount = nodes.Count / 9;
            progress?.Report($"Read {triCount} triangles. Building STEP body...");
            Console.WriteLine($@"Read {triCount} triangles from {inputFile}");

            // Build STEP body and write output
            var stepWriter = new StepWriter();
            int mergedEdgeCount = 0;
            token.ThrowIfCancellationRequested();
            stepWriter.BuildTriBody(nodes, tol, ref mergedEdgeCount);
            token.ThrowIfCancellationRequested();
            progress?.Report($"Writing STEP: {Path.GetFileName(outputFile)}...");
            stepWriter.WriteStep(outputFile);

            var mergedMsg = $"Merged {mergedEdgeCount} edges";
            Console.WriteLine(@mergedMsg);
            Console.WriteLine($@"Exported STEP file: {outputFile}");
            progress?.Report($"Done. {mergedMsg}. Saved: {outputFile}");

            return 0;
        }
    }
}