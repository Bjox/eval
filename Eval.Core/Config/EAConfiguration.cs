#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using System.IO;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Newtonsoft.Json;

namespace Eval.Core.Config
{
    [Serializable]
    public class EAConfiguration : IEAConfiguration
    {
        public int PopulationSize { get; set; }
        public double OverproductionFactor { get; set; }
        public int MaximumGenerations { get; set; } = int.MaxValue;
        public CrossoverType CrossoverType { get; set;  }
        public AdultSelectionType AdultSelectionType { get; set; }
        public ParentSelectionType ParentSelectionType { get; set; }
        public double CrossoverRate { get; set; }
        public double MutationRate { get; set; }
        public int TournamentSize { get; set; }
        public double TournamentProbability { get; set; }
        public double TargetFitness { get; set; }
        public EAMode Mode { get; set; }
        public int Elites { get; set; }
        public bool ReevaluateElites { get; set; } = false;
        public double RankSelectionMinProbability { get; set; }
        public double RankSelectionMaxProbability { get; set; }
        public bool CalculateStatistics { get; set; } = false;
        public bool MultiThreaded { get; set; } = false;
        public int SnapshotGenerationInterval { get; set; }
        public string SnapshotFilename { get; set; }
        public TimeSpan? MaxDuration { get; set; }


        public static EAConfiguration ReadConfigurationFromFile(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                return (EAConfiguration)serializer.Deserialize(file, typeof(EAConfiguration));
            }
        }

        public static EAConfiguration ParseFromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<EAConfiguration>(json);
        }

    }
}
