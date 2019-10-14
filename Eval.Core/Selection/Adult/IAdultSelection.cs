using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Selection.Adult
{
    public interface IAdultSelection
    {
        void SelectAdults(Population offspring, Population population, int n, bool maximizeFitness);
    }
}
