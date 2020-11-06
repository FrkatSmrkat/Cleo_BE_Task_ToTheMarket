using System;
using System.IO;
using System.Linq;

namespace MarketApp.HelpClasses
{
    public class TestFile
    {
        public string NameNoExtension { get; }

        public string InputPath { get; }

        public string OutputPath { get; }

        public long Input => _lazyInput.Value;

        public int OutputKindsCount => _lazyOutputKindsCount.Value;

        public long[] OutputNumbers => _lazyOutputNumbers.Value;

        private readonly Lazy<long> _lazyInput;
        private readonly Lazy<int> _lazyOutputKindsCount;
        private readonly Lazy<long[]> _lazyOutputNumbers;

        public TestFile(string inputFilePath)
        {
            var dir = Path.GetDirectoryName(inputFilePath);

            NameNoExtension = Path.GetFileNameWithoutExtension(inputFilePath);
            InputPath = inputFilePath;
            OutputPath = Path.Combine(dir, NameNoExtension + ".out");

            _lazyInput = new Lazy<long>(() => long.Parse(File.ReadAllLines(InputPath).First().Trim()));

            var lazyOutputLines = new Lazy<string[]>(() => File.ReadAllLines(OutputPath));

            _lazyOutputKindsCount = new Lazy<int>(() => int.Parse(lazyOutputLines.Value[0]));
            _lazyOutputNumbers = new Lazy<long[]>(() => lazyOutputLines.Value[1].Split(' ').Select(long.Parse).ToArray());
        }

        public static TestFile From(string inputFilePath)
        {
            return new TestFile(inputFilePath);
        }
    }
}