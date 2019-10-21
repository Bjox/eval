using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Selection.Parent
{
    public interface IParentSelection
    {
        IEnumerable<(IPhenotype, IPhenotype)> SelectParents(Population population, int n, EAMode mode, IRandomNumberGenerator random);
    }
}
