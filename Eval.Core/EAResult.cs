using Eval.Core.Models;
using System;

namespace Eval.Core
{
    [Serializable]
    public class EAResult
    {
        public IPhenotype Winner { get; set; }
        public Population EndPopulation { get; set; }
    }
}
