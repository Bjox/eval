using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    public enum TerminationReason
    {
        Aborted,
        DurationLimitReached,
        FitnessLimitReached,
        GenerationLimitReached
    }
}
