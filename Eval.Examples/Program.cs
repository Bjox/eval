using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eval.Core.Util.Roulette;
using Eval.Core.Util.EARandom;
using Eval.Core.Models;

namespace Eval.Examples
{


    class Program
    {
        static void Main(string[] args)
        {
            var pop = new Population(3);

            Console.WriteLine(pop.Count);
            pop.Add(new P());
            pop.GetMaxFitness();
            Console.WriteLine(pop.Count);

            pop.Fill(() => new P());

            pop.Add(new P());
            Console.WriteLine(pop.Count);
            pop.Add(new P());
            Console.WriteLine(pop.Count);

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }


    class P : IPhenotype
    {
        public bool IsEvaluated => throw new NotImplementedException();

        public Genotype Genotype => throw new NotImplementedException();

        public double Fitness => 1;

        public double Evaluate()
        {
            throw new NotImplementedException();
        }
    }
}
