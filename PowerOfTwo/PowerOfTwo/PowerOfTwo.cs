using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// DISCLAIMER: I noticed that during integer divisions, the result would automatically be converted to an integer - not knowing this beforehand was my mistake. Therefore, to ensure that all calculations are as thorough as possible (in the expense of being overzealous), all the method's main properties were set to 'double', even if the examples doesn't allow non-integer values (like 'Q')

namespace PowerOfTwo
{
    public class Facility
    {
        public string id { get; set; }
        public double K { get; set; }
        public double h { get; set; }
        public double e { get; set; }

        public double Q { get; set; }
        public double C { get; set; }

        public Facility(string p_id, double p_K, double p_h, double p_e)
        {
            id = p_id;
            K = p_K;
            h = p_h;
            e = p_e;
        }
        public Facility(Facility facility)
        {
            this.id = facility.id;
            this.K = facility.K;
            this.h = facility.h;
            this.e = facility.e;
            this.Q = facility.Q;
            this.C = facility.C;
        }
    }

    public class FacilityExtended : Facility
    {
        public FacilityExtended(Facility facility) : base(facility) { }

        public double Q_ast { get; set; } = 0.00;
        public double n { get; set; } = 0.00;
        public double m { get; set; } = 0.00;
        public double p { get; set; } = 0.00;

        public double C_ast(double demand)
        {
            return Math.Round(this.K * (demand / this.Q_ast) + this.e * this.Q_ast / 2, 2);
        }
    }

    public abstract class IInstance
    {
        public double demand { get; set; } = 0.00;
        public int size { get; set; } = 0;

        public abstract void print(string message, int count = 0);
    }

    public class Instance : IInstance
    {
        public List<Facility> facilities { get; set; }

        public Instance(List<Facility> p_facilities, double p_demand)
        {
            facilities = p_facilities;
            demand = p_demand;
            size = p_facilities.Count;
        }
        public Instance(string filename)
        {
            try
            {
                StreamReader stream = new StreamReader(filename);

                facilities = new List<Facility>();
                demand = Convert.ToDouble(stream.ReadLine());
                while (!stream.EndOfStream)
                {
                    var line_data = stream.ReadLine().Split(';');
                    var data = new { id = line_data[0], K = Convert.ToDouble(line_data[1]), h = Convert.ToDouble(line_data[2]), e = Convert.ToDouble(line_data[3]) };
                    facilities.Add(new Facility(data.id, data.K, data.h, data.e));
                }
                size = facilities.Count;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when reading file '{filename}': {e.Message}");
                throw;
            }
        }

        override public void print(string message = "Printing instance...", int count = 0) // 'count' does nothing here
        {
            Console.WriteLine(message);
            Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10}", "ID", "K", "h", "e");
            facilities.ForEach(facility => Console.WriteLine("{0,-10} {1,-10:N2} {2,-10:N2} {3,-10:N2}", facility.id, facility.K, facility.h, facility.e));
        }
    }

    public class Solution : IInstance
    {
        public List<FacilityExtended> facilities { get; set; }
        public double LB { get; set; } = 0.00;
        public double UB { get; set; } = 0.00;
        public double Kipi { get; set; } = 0.00;
        public double eipi { get; set; } = 0.00;
        public double? Qast_aux { get; set; } = null;

        public Solution(Instance instance)
        {
            facilities = new List<FacilityExtended>();
            for (int i = 0; i < instance.facilities.Count; i++)
            {
                facilities.Add(new FacilityExtended(instance.facilities[i]));
            }
            demand = instance.demand;
            size = instance.size;
        }

        override public void print(string message, int count = 0) // 'count' does not retrieve specific iteration's information, it is only used for tracking while executing the algorithm
        {
            Console.WriteLine(message);
            Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10}", "ID", "K", "h", "e");
            facilities.ForEach(facility => Console.WriteLine("{0,-10} {1,-10:N2} {2,-10:N2} {3,-10:N2}", facility.id, facility.K, facility.h, facility.e));
            Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10}", "Iteration", "UB", "LB", "GAP(%)");
            Console.WriteLine("{0,-10} {1,-10:N2} {2,-10:N2} {3,-10:N2}", count, UB, LB, (UB - LB) / LB * 100);
        }
    }

    class PowerOfTwoSolver
    {
        private readonly Instance instance;
        private Solution solution;

        public PowerOfTwoSolver(Instance p_instance)
        {
            instance = p_instance;
        }

        private void initializeSolution()
        {
            Console.WriteLine("Initializing solution...");
            solution = new Solution(instance);
            instance.print("Printing template instance:");
        }

        public void run()
        {
            initializeSolution();
            try
            {
                Console.WriteLine("\n---------------- Starting algorithm ----------------");
                phase1();
                phase2();
                Console.WriteLine("\nFinishing 'PowerOfTwo' solver...");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in 'PowerOfTwoSolver:' {e.Message}");
                throw;
            }
        }

        private void phase1()
        {
            try
            {
                phase1_step1();
                phase1_step2();
                solution.print("Printing 'Phase1' solution...");
            }
            catch (Exception e)
            {
                throw new Exception($"Exception thrown in 'phase1': {e.Message}", e);
            }
        }

        private void phase1_step1()
        {
            for (int i = 0; i < solution.size - 1; i++)
            {
                if ((instance.facilities[i].K / instance.facilities[i].e) < (instance.facilities[i + 1].K / instance.facilities[i + 1].e))
                    joinFacilities(i);
            }
            Console.WriteLine("Step1 finished...");
        }

        private void phase1_step2()
        {
            for (int i = 0; i < solution.size; i++)
            {
                double temp_Q = Math.Sqrt((2 * solution.facilities[i].K * solution.demand) / solution.facilities[i].e);
                if (temp_Q < 1)
                {
                    solution.facilities[i].Q = 1;
                }
                else if ((temp_Q / Convert.ToInt32(temp_Q)) <= ((Convert.ToInt32(temp_Q) + 1) / temp_Q))
                {
                    solution.facilities[i].Q = Convert.ToInt32(temp_Q);
                }
                else
                {
                    solution.facilities[i].Q = Convert.ToInt32(temp_Q) + 1;
                }

                // not sure if 'round' or 'int' casting are the right approachs in this phase... needs to be verified later

                solution.facilities[i].C = Math.Round(solution.facilities[i].K * (solution.demand / solution.facilities[i].Q) + solution.facilities[i].e * (solution.facilities[i].Q / 2), 2);
                solution.LB += solution.facilities[i].C;
            }
            Console.WriteLine("Step2 finished...");
        }

        private void joinFacilities(int i)
        {
            Console.WriteLine($"Joining facilities {solution.facilities[i].id} and {solution.facilities[i + 1].id}...");
            solution.facilities[i].id += " + " + solution.facilities[i + 1].id;
            solution.facilities[i].K += solution.facilities[i + 1].K;
            solution.facilities[i].e += solution.facilities[i + 1].e;

            Console.WriteLine($"Removing facility {solution.facilities[i + 1].id}...");
            solution.facilities.RemoveAt(i + 1);
            --solution.size;
        }

        private void phase2()
        {
            try
            {
                solution.facilities.LastOrDefault().p = 1;
                int count = 0;
                while (!stop())
                {
                    phase2_part1();
                    phase2_part2();
                    solution.print($"\nPrinting 'Phase 2' iteration = {++count} results...", count);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Exception thrown in 'phase2': {e.Message}", e);
            }
        }

        private void phase2_part1()
        {
            solution.facilities.ForEach(facility => facility.Q_ast = facility.Q);
            solution.facilities.ForEach(facility => facility.p = 1);

            for (int i = solution.size - 2; i >= 0; i--)
            {
                if (solution.facilities[i].Q_ast < solution.facilities[i + 1].Q_ast)
                {
                    solution.facilities[i].Q_ast = solution.facilities[i + 1].Q_ast;
                    solution.facilities[i].n = 1;
                }
                else
                {
                    while (!((Math.Pow(2, solution.facilities[i].m) * solution.facilities[i + 1].Q_ast <= solution.facilities[i].Q_ast) && (solution.facilities[i].Q_ast < Math.Pow(2, solution.facilities[i].m + 1) * solution.facilities[i].Q_ast)))
                    {
                        ++solution.facilities[i].m;
                    }

                    if (solution.facilities[i].Q_ast / (Math.Pow(2, solution.facilities[i].m) * solution.facilities[i + 1].Q_ast) <= Math.Pow(2, solution.facilities[i].m + 1) * solution.facilities[i + 1].Q_ast / solution.facilities[i].Q_ast)
                    {
                        solution.facilities[i].n = Math.Pow(2, solution.facilities[i].m);
                    }
                    else
                    {
                        solution.facilities[i].n = Math.Pow(2, solution.facilities[i].m + 1);
                    }

                    solution.facilities[i].Q_ast = solution.facilities[i].n * solution.facilities[i + 1].Q_ast;
                }
            }
            for (int i = solution.facilities.Count - 2; i >= 0; i--)
            {
                solution.facilities[i].p = solution.facilities[i].n * solution.facilities[i + 1].p;
            }
            solution.facilities.ForEach(facility => facility.C = facility.C_ast(solution.demand));
            solution.UB = solution.facilities.Sum(facility => facility.C);
        }

        private void phase2_part2()
        {
            for (int i = 0; i < solution.facilities.Count; i++)
            {
                solution.Kipi += solution.facilities[i].K / solution.facilities[i].p;
                solution.eipi += solution.facilities[i].e * solution.facilities[i].p;
            }
            solution.Qast_aux = Math.Sqrt(2 * solution.Kipi * solution.demand / solution.eipi);

            if (solution.Qast_aux < 1)
                solution.Qast_aux = 1;
            if (solution.Qast_aux / Convert.ToInt32(solution.Qast_aux) <= (Convert.ToInt32(solution.Qast_aux) + 1) / solution.Qast_aux)
                solution.Qast_aux = Convert.ToInt32(solution.Qast_aux);
            else
                solution.Qast_aux = Convert.ToInt32(solution.Qast_aux) + 1;
        }

        private bool stop()
        {
            if (solution.Qast_aux == null)
            {
                return false;
            }
            if (solution.Qast_aux != solution.facilities.LastOrDefault().Q_ast)
            {
                solution.facilities.LastOrDefault().Q_ast = (double)solution.Qast_aux;
                solution.facilities.LastOrDefault().Q = (double)solution.Qast_aux;
                return false;
            }
            return true;
        }


    }
     // ------------------------------------------------------------------------------------
    class PowerOfTwo
    {
        static int Main(string[] args)
        {
            try
            {
                CommandLine.Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(RunOptions)
                    .WithNotParsed(HandleParseError);
            }
            catch (Exception)
            { 
                return 1;
            }
            return 0;
        }
        static void RunOptions(Options opts)
        {
            try
            {
                // base declarations
                Instance instance;
                PowerOfTwoSolver solver;
                
                // parsing
                if (opts.filename != null)
                {
                    Console.WriteLine($"Attempting to read file '{opts.filename}'...");
                    instance = new Instance(opts.filename);
                }
                else if (opts.size != null)
                {
                    instance = (Instance)Options.Methods.ManualInstanceInput(opts.size);
                }
                else
                {
                    Console.WriteLine($"Using sample instance provided with application...");
                    instance = new Instance(SAMPLE_INSTANCE.facilities, SAMPLE_INSTANCE.demand);
                }

                // algorithm
                solver = new PowerOfTwoSolver(instance);
                solver.run();                
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception detected in 'Main': {e.Message}");
                throw;
            }
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            // default
        }
    }
}
