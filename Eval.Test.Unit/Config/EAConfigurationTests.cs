#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval.Test.Unit.Config
{
    [TestClass]
    public class EAConfigurationTests
    {
        [TestMethod]
        public void ReadConfigFromJsonStringTest()
        {
            var json =
@"{
    ""PopulationSize"": 100,
    ""OverproductionFactor"": 1.5,
    ""MaximumGenerations"": 500,
    ""CrossoverType"": ""Uniform"",
    ""AdultSelectionType"": ""Overproduction"",
    ""ParentSelectionType"": ""FitnessProportionate"",
    ""CrossoverRate"": 0.8,
    ""MutationRate"": 0.02,
    ""TournamentSize"": 10,
    ""TournamentProbability"": 0.5,
    ""TargetFitness"": 1.0,
    ""Mode"": ""MaximizeFitness"",
    ""Elites"": 1,
    ""ReevaluateElites"": true,
    ""RankSelectionMinProbability"": 0.5,
    ""RankSelectionMaxProbability"": 1.5
}";

            var config = EAConfiguration.ParseFromJsonString(json);
            
            config.PopulationSize.Should().Be(100);
            config.OverproductionFactor.Should().Be(1.5);
            config.MaximumGenerations.Should().Be(500);
            config.CrossoverType.Should().Be(CrossoverType.Uniform);
            config.AdultSelectionType.Should().Be(AdultSelectionType.Overproduction);
            config.ParentSelectionType.Should().Be(ParentSelectionType.FitnessProportionate);
            config.CrossoverRate.Should().Be(0.8);
            config.MutationRate.Should().Be(0.02);
            config.TournamentSize.Should().Be(10);
            config.TournamentProbability.Should().Be(0.5);
            config.TargetFitness.Should().Be(1.0);
            config.Mode.Should().Be(EAMode.MaximizeFitness);
            config.Elites.Should().Be(1);
            config.ReevaluateElites.Should().BeTrue();
            config.RankSelectionMinProbability.Should().Be(0.5);
            config.RankSelectionMaxProbability.Should().Be(1.5);
        }

        [TestMethod]
        public void ReadConfigFromFileTest()
        {
            var path = "TestFiles/TestConfig.json";
            var config = EAConfiguration.ReadConfigurationFromFile(path);

            config.PopulationSize.Should().Be(100);
            config.OverproductionFactor.Should().Be(1.5);
            config.MaximumGenerations.Should().Be(500);
            config.CrossoverType.Should().Be(CrossoverType.Uniform);
            config.AdultSelectionType.Should().Be(AdultSelectionType.Overproduction);
            config.ParentSelectionType.Should().Be(ParentSelectionType.FitnessProportionate);
            config.CrossoverRate.Should().Be(0.8);
            config.MutationRate.Should().Be(0.02);
            config.TournamentSize.Should().Be(10);
            config.TournamentProbability.Should().Be(0.5);
            config.TargetFitness.Should().Be(1.0);
            config.Mode.Should().Be(EAMode.MaximizeFitness);
            config.Elites.Should().Be(1);
            config.ReevaluateElites.Should().BeTrue();
        }
    }

    
}
