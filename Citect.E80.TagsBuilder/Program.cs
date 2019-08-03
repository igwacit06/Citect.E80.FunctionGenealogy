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
                dbfautomation.ExecuteGeneration();

                Console.WriteLine("dbf csv build complete check output folder");
            }
            Console.ReadKey();
        }
    }
}
