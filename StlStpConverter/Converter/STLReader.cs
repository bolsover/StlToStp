using System;
using System.Buffers; // <-- requires System.Buffers NuGet
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bolsover.Converter
{
    public abstract class StlReader
    {
        public static async Task<List<double>> ReadStlAsync(string fileName, CancellationToken token,
            IProgress<string> progress)
        {
            var nodes = new List<double>();

            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($"Failed to open STL file: {fileName}");
                    return nodes;
                }

                var fileInfo = new FileInfo(fileName);
                var fileSize = fileInfo.Length;
                if (fileSize < 15)
                {
                    Console.WriteLine($"Invalid STL file: {fileName}");
                    return nodes;
                }

                // Peek first 5 bytes for "solid"
                var firstBytes = new byte[5];
                using (var fsPeek = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read,
                           bufferSize: 64 * 1024, useAsync: true))
                {
                    var bytesRead = await fsPeek.ReadAsync(firstBytes, 0, 5, token).ConfigureAwait(false);
                    if (bytesRead < 5)
                    {
                        Console.WriteLine($"Invalid STL file: {fileName}");
                        return nodes;
                    }
                }

                var firstWord = Encoding.ASCII.GetString(firstBytes);

                // Heuristic: ASCII if starts with "solid" and LooksLikeAsciiAsync says true; else binary
                if (firstWord == "solid")
                {
                    var looksAscii = await LooksLikeAsciiAsync(fileName, token).ConfigureAwait(false);
                    if (looksAscii)
                    {
                        // Use your existing ASCII parser
                        nodes = await ReadStlAsciiAsync(fileName, token, progress).ConfigureAwait(false);
                        return nodes;
                    }
                }

                // Binary fast path using Buffer.BlockCopy + ArrayPool<float>
                nodes = await ReadBinaryWithBlockCopyAsync(fileName, token, progress).ConfigureAwait(false);
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($"Access denied: {uaEx.Message}");
            }
            catch (OperationCanceledException)
            {
                // Swallow cancellation (propagate if you prefer)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return nodes;

            // Local function: binary reader using pooled float[] and Buffer.BlockCopy
            static async Task<List<double>> ReadBinaryWithBlockCopyAsync(string path, CancellationToken ct, IProgress<string> prog)
            {
                const int TriBytes = 50;     // 12 floats (48 bytes) + 2 attribute bytes
                const int FloatsPerTri = 12; // 3 normal + 9 vertex

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: 64 * 1024, useAsync: true);

                // Read 80-byte header + 4-byte triangle count
                var header = new byte[84];
                int hdrRead = await ReadExactlyAsync(fs, header, 0, 84, ct).ConfigureAwait(false);
                if (hdrRead != 84) throw new EndOfStreamException("Short STL header");

                uint tris = BitConverter.ToUInt32(header, 80);
                long remainingBytes = (long)tris * TriBytes;

                var result = new List<double>(checked((int)tris * 9)); // 9 coordinates per triangle

                // Choose a reasonable number of triangles per chunk to limit working set
                // Example: ~8 MB chunks => 8*1024*1024 / 50 ≈ 167,772 tris per chunk (upper bound).
                // We'll clamp to something smaller to keep float buffers modest.
                const int MaxTrisPerChunk = 80_000;

                long trianglesRead = 0;
                while (remainingBytes > 0)
                {
                    ct.ThrowIfCancellationRequested();

                    int trisThisChunk = (int)Math.Min(MaxTrisPerChunk, remainingBytes / TriBytes);
                    int bytesThisChunk = trisThisChunk * TriBytes;
                    if (bytesThisChunk == 0) break;

                    // Read exactly the bytes for this chunk
                    var chunk = ArrayPool<byte>.Shared.Rent(bytesThisChunk);
                    try
                    {
                        int got = await ReadExactlyAsync(fs, chunk, 0, bytesThisChunk, ct).ConfigureAwait(false);
                        if (got != bytesThisChunk) throw new EndOfStreamException("Unexpected end of STL file");

                        // Rent float buffer for the 12 floats per triangle (normal + verts)
                        int totalFloats = trisThisChunk * FloatsPerTri; // 12*tris
                        var floats = ArrayPool<float>.Shared.Rent(totalFloats);
                        try
                        {
                            // Copy 48 bytes of floats per triangle (skip 2-byte attribute) into float buffer
                            int src = 0; // in bytes
                            int dst = 0; // in bytes into float[]
                            for (int t = 0; t < trisThisChunk; t++)
                            {
                                Buffer.BlockCopy(chunk, src, floats, dst, 48); // 12 floats
                                src += TriBytes; // advance by 50
                                dst += 48;       // advance by 48
                            }

                            // Consume the floats: skip the 3 normal floats and add the 9 vertex floats
                            int f = 0;
                            for (int t = 0; t < trisThisChunk; t++)
                            {
                                f += 3; // skip normal
                                result.Add(floats[f++]); result.Add(floats[f++]); result.Add(floats[f++]);
                                result.Add(floats[f++]); result.Add(floats[f++]); result.Add(floats[f++]);
                                result.Add(floats[f++]); result.Add(floats[f++]); result.Add(floats[f++]);
                            }
                        }
                        finally
                        {
                            ArrayPool<float>.Shared.Return(floats);
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(chunk);
                    }

                    remainingBytes -= bytesThisChunk;
                    trianglesRead += trisThisChunk;

                    if ((trianglesRead & 0x1FFF) == 0) // every ~8192 tris
                        prog?.Report($"Read {trianglesRead} triangles so far (binary)...");
                }

                return result;
            }

            // Helper to read an exact number of bytes (like Stream.ReadExactly in newer .NET)
            static async Task<int> ReadExactlyAsync(Stream s, byte[] buffer, int offset, int count, CancellationToken ct)
            {
                int total = 0;
                while (total < count)
                {
                    int read = await s.ReadAsync(buffer, offset + total, count - total, ct).ConfigureAwait(false);
                    if (read == 0) break;
                    total += read;
                }
                return total;
            }
        }
        
        private static async Task<List<double>> ReadStlAsciiAsync(string fileName, CancellationToken token,
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
                var lineCount = 0;
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
        
  
        public static async Task<int> ConvertToStp(string inputFile, string outputFile, double tol = 1e-6)
        {
            return await ConvertToStp(inputFile, outputFile, tol, CancellationToken.None, null);
        }

        private static async Task<int> ConvertToStp(string inputFile, string outputFile, double tol, CancellationToken token,
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
               // Console.WriteLine(@msg);
                return 1;
            }

            var triCount = nodes.Count / 9;
            progress?.Report($"Read {triCount} triangles. Building STEP body...");
            //Console.WriteLine($@"Read {triCount} triangles from {inputFile}");

            // Build STEP body and write output
            var stepWriter = new StepWriter();
            var mergedEdgeCount = 0;
            token.ThrowIfCancellationRequested();
            stepWriter.BuildTriangularBody(nodes, tol, ref mergedEdgeCount);
            token.ThrowIfCancellationRequested();
            progress?.Report($"Writing STEP: {Path.GetFileName(outputFile)}...");
            stepWriter.WriteStep(outputFile);

            var mergedMsg = $"Merged {mergedEdgeCount} edges";
            progress?.Report(@mergedMsg);
            progress?.Report($@"Exported STEP file: {outputFile}");
            progress?.Report($"Done. {mergedMsg}. Saved: {outputFile}");

            return 0;
        }
    }
}