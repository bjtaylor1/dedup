using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dedup
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                string path;
                if (args.Length < 1 || !File.Exists(path = args[0]))
                {
                    await Console.Out.WriteLineAsync($"Usage: dedup [csproj file]");
                    return 1;
                }

                var dedupMatchRegex = new Regex(@"<(?:Compile|Content)\s+Include\s*=\s*""(.+)""\s*/>");

                var files = new HashSet<string>();
                string tempPath = path + ".tmp";
                using (var inputStream = new StreamReader(path, true))
                using (var outputStream = new StreamWriter(tempPath, false, inputStream.CurrentEncoding))
                {
                    string line;
                    while ((line = await inputStream.ReadLineAsync()) != null)
                    {
                        var match = dedupMatchRegex.Match(line);
                        bool writeLine = true;
                        if (match.Success)
                        {
                            var file = match.Groups[1].Value;
                            if (!files.Add(file))
                            {
                                await Console.Out.WriteLineAsync($"Removing duplicate {file}");
                                writeLine = false;
                            }
                        }

                        if (writeLine)
                        {
                            await outputStream.WriteLineAsync(line);
                        }
                    }
                }
                File.Move(tempPath, path, true);

                return 0;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                return 2;
            }
        }
    }
}
