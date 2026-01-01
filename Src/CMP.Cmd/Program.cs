using CMP.Lib.Rpt;
using CMP.Lib.Analysis;

internal class Program
{
    private static void Main(string[] args)
    {
        DirectoryProcessor directoryProcessor = new(new ConsoleReportService());

        // Write greeting message to the console
        Console.WriteLine("Directory Comparator Program");
        if (args.Length == 3)
        {
            string sourceDirPath = args[0];
            string targetDirPath = args[1];
            string reportFilePath = args[2];

            if (directoryProcessor.CompareDirectoriesContents(sourceDirPath, targetDirPath, out string resultJson))
            {
                System.IO.File.WriteAllText(reportFilePath, resultJson);
            }
            else
            {
                Console.WriteLine("Failed to compare directories.");
                Console.WriteLine(resultJson);
            }
        }
        else if (args.Length == 2)
        {
            string dirPath = args[0];
            string outputFilePath = args[1];

            if (directoryProcessor.BuildDirectoryContent(dirPath, out string jsonString))
            {
                System.IO.File.WriteAllText(outputFilePath, jsonString);
            }
            else
            {
                Console.WriteLine("Failed to build directory content.");
                Console.WriteLine(jsonString);
            }
        }
        else
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  To list directory content: CMP.Cmd <dirPath> <outputFilePath>");
            Console.WriteLine("  To compare directories: CMP.Cmd <sourceDirPath> <targetDirPath> <reportFilePath>");
        }
    }
}
