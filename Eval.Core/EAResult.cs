﻿using Eval.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core
{
    [Serializable]
    public class EAResult
    {
        public IPhenotype Winner { get; set; }
        public Population EndPopulation { get; set; }
    }
}
