using System.IO;
using System.Linq;
using System.Reflection;

namespace MarketApp.HelpClasses
{
    public static class Helpers
    {
        public static TestFile[] ReadFilesForLevel(int level)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filesDir = Path.Combine(dir, "TestFiles");

            var filePaths = Directory.GetFiles(filesDir, "*.in");
            
            var files = filePaths.Select(TestFile.From).ToList();

            var levelAsString = level.ToString("00");

            var levelFiles = files.Where(file => file.NameNoExtension.StartsWith(levelAsString)).ToArray();

            return levelFiles;
        }

        public static string CutIfLong(this string text, int maxLength)
        {
            if (text.Length <= maxLength)
            {
                return text;
            }
            else
            {
                return text.Substring(0, maxLength) + " ...";
            }
        }
    }
}
