using Citect.E80.BulkDBFUpdates;
using System;

namespace Citect.E80.TagsBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbfautomationtest = new DBFAutomation();
            Console.WriteLine("dbf csv build complete check output folder");
            Console.ReadKey();
        }
    }
}
