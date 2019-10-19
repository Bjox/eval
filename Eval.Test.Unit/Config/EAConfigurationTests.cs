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
    ""Crossover"": ""Uniform"",
    ""AdultSelection"": ""Overproduction"",
    ""ParentSelection"": ""FitnessProportionate"",
    ""CrossoverRate"": 0.8,
    ""MutationRate"": 0.02,
    ""TournamentSize"": 10,
    ""TournamentProbability"": 0.5,
    ""TargetFitness"": 1.0,
    ""Mode"": ""MaximizeFitness"",
    ""Elites"": 1,
    ""ReevaluateElites"": true
}";

            var config = EAConfiguration.ParseFromJsonString(json);

            config.PopulationSize.Should().Be(100);
            config.OverproductionFactor.Should().Be(1.5);
            config.MaximumGenerations.Should().Be(500);
            config.Crossover.Should().Be(Crossover.Uniform);
            config.AdultSelection.Should().Be(AdultSelection.Overproduction);
            config.ParentSelection.Should().Be(ParentSelection.FitnessProportionate);
            config.CrossoverRate.Should().Be(0.8);
            config.MutationRate.Should().Be(0.02);
            config.TournamentSize.Should().Be(10);
            config.TournamentProbability.Should().Be(0.5);
            config.TargetFitness.Should().Be(1.0);
            config.Mode.Should().Be(EAMode.MaximizeFitness);
            config.Elites.Should().Be(1);
            config.ReevaluateElites.Should().BeTrue();
        }

        [TestMethod]
        public void ReadConfigFromFileTest()
        {
            var path = "TestFiles/TestConfig.json";
            var config = EAConfiguration.ReadConfigurationFromFile(path);

            config.PopulationSize.Should().Be(100);
            config.OverproductionFactor.Should().Be(1.5);
            config.MaximumGenerations.Should().Be(500);
            config.Crossover.Should().Be(Crossover.Uniform);
            config.AdultSelection.Should().Be(AdultSelection.Overproduction);
            config.ParentSelection.Should().Be(ParentSelection.FitnessProportionate);
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
