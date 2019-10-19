using System;
using System.Collections.Generic;
using System.Text;
using Eval.Core.Config;
using Eval.Core.Models;

namespace Eval.Core.Selection.Adult
{
    public class GenerationalReplacement : IAdultSelection
    {
        public void SelectAdults(Population offspring, Population population, int n, EAMode eamode)
        {
            population.Clear();
            for (int i = 0; i < n; i++)
            {
                population.Add(offspring[i]);
            }
        }
    }
}
