using Citect.E80.BulkDBFUpdates;
using System;

namespace Citect.E80.TagsBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbfautomation = new DBFAutomation();
            if (dbfautomation.ExcelError)
                Console.WriteLine("Excel read error check if excel worksheet closed?");
            else
            {
                Console.WriteLine("Starting Tag Builder");
                dbfautomation.ExecuteGeneration();

                if (dbfautomation.Validate)
                {
                    var starttime = DateTime.Now;
                    Console.WriteLine("Performing validation. please wait...");
                    if (!dbfautomation.PerformValidate())
                        Console.WriteLine("errors encountered, check logs and try again");
                    else
                        Console.WriteLine("validation complete. Elapsed time {0} min", DateTime.Now.Subtract(starttime).Minutes);
                }

                Console.WriteLine("Tags build complete, check output folder");
            }
            Console.ReadKey();
        }
    }
}
