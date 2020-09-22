using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerOfTwo
{
    public static class SAMPLE_INSTANCE
    {
        public static readonly List<Facility> facilities = new List<Facility>{
            new Facility("1", 161.59, 1.24, 1.24),
            new Facility("2", 117.28, 2.64, 1.40),
            new Facility("3", 130.29, 3.27, 0.63),
            new Facility("4", 52.87, 4.34, 1.07)
            };
        public static readonly double demand = 3255;
        public static readonly int size = 4;
    }

    class Options
    {
        [Option('r', "read", SetName = "input", Required = false, HelpText = "Input file to be processed. Must use the default file structure (program will not parse or correct wrong inputs).")]
        public string filename { get; set; }

        [Option('i', "interactive", SetName = "input", Required = false, HelpText = "Manual input for the problem's data. Takes as input a supply chain size (int).")]
        public int? size { get; set; }

        public static class Methods
        {
            public static IInstance ManualInstanceInput(int? size)
            {
                try
                {
                    Console.WriteLine("Manual instance input started. Press 'Control + C' to abort");
                    List<Facility> facilities = new List<Facility>((int)size);
                    int i = 0;
                    while (i < size)
                    {
                        string id = Convert.ToString(i + 1);
                        try
                        {
                            Console.WriteLine($"Currently working on facility {i + 1}...");
                            Console.WriteLine("Please set the facility's 'K' value (double):");
                            double K = Convert.ToDouble(Console.ReadLine());
                            Console.WriteLine("Please set the facility's 'h' value (double):");
                            double h = Convert.ToDouble(Console.ReadLine());
                            Console.WriteLine("Please set the facility's 'e' value (double):");
                            double e = Convert.ToDouble(Console.ReadLine());
                            facilities.Add(new Facility(id, K, h, e));
                            ++i;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error when parsing input from user: {e.Message}");
                        }
                    }
                    Console.WriteLine("Please set the demand value (int):");
                    int demand = Convert.ToInt32(Console.ReadLine());
                    return new Instance(facilities, demand);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception detected in 'ManualInstanceInput': {e.Message}");
                    throw;
                }
            }
        }
    }
}