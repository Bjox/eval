using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Eval.Core
{

    [Serializable]
    public abstract class EA
    {
        public IEAConfiguration EAConfiguration { get; set; }

        [field: NonSerialized]
        public event Action<int> NewGenerationEvent;
        [field: NonSerialized]
        public event Action<IPhenotype> NewBestFitnessEvent;
        [field: NonSerialized]
        public event Action<double> FitnessLimitReachedEvent;
        [field: NonSerialized]
        public event Action<int> GenerationLimitReachedEvent;
        [field: NonSerialized]
        public event Action<IPhenotype> PhenotypeEvaluatedEvent;
        [field: NonSerialized]
        public event Action AbortedEvent;
        [field: NonSerialized]
        public event Action<PopulationStatistics> PopulationStatisticsCalculated;

        protected List<PopulationStatistics> PopulationStatistics { get; private set; }
        [JsonConverter(typeof(EAConverter))]
        protected List<IPhenotype> Elites;

        protected IPhenotype GenerationalBest;
        protected IPhenotype Best;
        protected bool Abort;
        protected IParentSelection ParentSelection;
        protected IAdultSelection AdultSelection;
        protected IRandomNumberGenerator RNG;
        protected int Generation { get; private set; }
        private bool _newRun;
        private int _offspringSize;
        private Population _offspring;

        protected Population Population { get; private set; }

        public EA(IEAConfiguration configuration, IRandomNumberGenerator rng)
        {
            EAConfiguration = configuration;
            RNG = rng;

            PopulationStatistics = new List<PopulationStatistics>(512);
            Elites = new List<IPhenotype>(Math.Max(EAConfiguration.Elites, 0));
            AdultSelection = CreateAdultSelection();
            ParentSelection = CreateParentSelection();

            if (EAConfiguration.WorkerThreads > 1)
            {
                ThreadPool.SetMinThreads(EAConfiguration.WorkerThreads, EAConfiguration.IOThreads);
                ThreadPool.SetMaxThreads(EAConfiguration.WorkerThreads, EAConfiguration.IOThreads);
            }

            _newRun = true;
        }

        protected abstract IPhenotype CreateRandomPhenotype();
        protected abstract IPhenotype CreatePhenotype(IGenotype genotype);

        /// <summary>
        /// Creates a new population filled with phenotypes from CreateRandomPhenotype method.
        /// Override this method to seed the EA with a specific population.
        /// </summary>
        /// <param name="populationSize"></param>
        /// <returns></returns>
        protected virtual Population CreateInitialPopulation(int populationSize)
        {
            var population = new Population(populationSize);
            population.Fill(CreateRandomPhenotype);
            return population;
        }

        protected virtual IAdultSelection CreateAdultSelection()
        {
            switch (EAConfiguration.AdultSelectionType)
            {
                case AdultSelectionType.GenerationalMixing:
                    return new GenerationalMixingAdultSelection(RNG);
                case AdultSelectionType.GenerationalReplacement:
                    return new GenerationalReplacementAdultSelection();
                case AdultSelectionType.Overproduction:
                    return new OverproductionAdultSelection(RNG);
                default:
                    throw new NotImplementedException($"AdultSelectionType {EAConfiguration.AdultSelectionType}");
            }
        }

        protected virtual IParentSelection CreateParentSelection()
        {
            switch (EAConfiguration.ParentSelectionType)
            {
                case ParentSelectionType.FitnessProportionate:
                    return new ProportionateParentSelection();
                case ParentSelectionType.Rank:
                    return new RankParentSelection();
                case ParentSelectionType.SigmaScaling:
                    return new SigmaScalingParentSelection();
                case ParentSelectionType.Tournament:
                    return new TournamentParentSelection(EAConfiguration);
                default:
                    throw new NotImplementedException($"ParentSelectionType {EAConfiguration.ParentSelectionType}");
            }
        }

        public EAResult Evolve()
        {
            if (_newRun)
            {
                Population = CreateInitialPopulation(EAConfiguration.PopulationSize);
                _offspringSize = (int)(EAConfiguration.PopulationSize * Math.Max(EAConfiguration.OverproductionFactor, 1));
                _offspring = new Population(_offspringSize);

                Population.Evaluate(EAConfiguration.ReevaluateElites, PhenotypeEvaluatedEvent);
                Generation = 1;

                Best = null;
                _newRun = true;
            }

            while (true)
            {
                Population.Sort(EAConfiguration.Mode);
                var generationBest = Population.First();

                if (IsBetterThan(generationBest, Best))
                {
                    Best = generationBest;
                    NewBestFitnessEvent?.Invoke(Best);
                }

                NewGenerationEvent?.Invoke(Generation);
                
                CalculateStatistics(Population);

                if (!RunCondition(Generation))
                {
                    break;
                }
                
                Elites.Clear();
                for (int i = 0; i < EAConfiguration.Elites; i++)
                    Elites.Add(Population[i]);
                
                var parents = ParentSelection.SelectParents(Population, _offspringSize - Elites.Count, EAConfiguration.Mode, RNG);
                
                _offspring.Clear();
                foreach (var couple in parents)
                {
                    IGenotype geno1 = couple.Item1.Genotype;
                    IGenotype geno2 = couple.Item2.Genotype;

                    IGenotype newgeno;
                    if (RNG.NextDouble() < EAConfiguration.CrossoverRate)
                        newgeno = geno1.CrossoverWith(geno2, EAConfiguration.CrossoverType, RNG);
                    else
                        newgeno = geno1.Clone();

                    newgeno.Mutate(EAConfiguration.MutationRate, RNG);
                    var child = CreatePhenotype(newgeno);
                    _offspring.Add(child);
                }
                
                CalculateFitnesses(_offspring);
                
                AdultSelection.SelectAdults(_offspring, Population, EAConfiguration.PopulationSize - Elites.Count, EAConfiguration.Mode);
                
                for (int i = 0; i < EAConfiguration.Elites; i++)
                    Population.Add(Elites[i]);

                Generation++;
            }

            return new EAResult
            {
                Winner = Population[0],
                EndPopulation = Population
            };
        }

        private bool IsBetterThan(IPhenotype subject, IPhenotype comparedTo)
        {
            if (comparedTo == null)
            {
                return true;
            }
            switch (EAConfiguration.Mode)
            {
                case EAMode.MaximizeFitness:
                    return subject.Fitness > comparedTo.Fitness;
                case EAMode.MinimizeFitness:
                    return subject.Fitness < comparedTo.Fitness;
                default:
                    throw new NotImplementedException($"IsBetterThan not implemented for EA mode {EAConfiguration.Mode}");
            }
        }

        protected virtual bool RunCondition(int generation)
        {
            if (Abort)
            {
                AbortedEvent?.Invoke();
                return false;
            }
            if (generation >= EAConfiguration.MaximumGenerations)
            {
                GenerationLimitReachedEvent?.Invoke(generation);
                return false;
            }
            if ((EAConfiguration.Mode == EAMode.MaximizeFitness && Best.Fitness >= EAConfiguration.TargetFitness) ||
                (EAConfiguration.Mode == EAMode.MinimizeFitness && Best.Fitness <= EAConfiguration.TargetFitness))
            {
                FitnessLimitReachedEvent?.Invoke(Best.Fitness);
                return false;
            }
            return true;
        }

        protected virtual void CalculateStatistics(Population population)
        {
            if (!EAConfiguration.CalculateStatistics)
                return;

            var popstats = population.CalculatePopulationStatistics();
            PopulationStatisticsCalculated?.Invoke(popstats);
            PopulationStatistics.Add(popstats);
        }

        protected virtual void CalculateFitnesses(Population population)
        {
            if (EAConfiguration.WorkerThreads <= 1)
            {
                foreach (var p in population)
                {
                    p.Evaluate();
                    PhenotypeEvaluatedEvent?.Invoke(p);
                }
            }
            else
            {
                using (var countdownEvent = new CountdownEvent(population.Count))
                {
                    foreach (var p in population)
                    {
                        ThreadPool.QueueUserWorkItem(FitnessWorker, new object[] { p, countdownEvent });
                    }
                    countdownEvent.Wait();
                }
            }
        }

        private void FitnessWorker(object state)
        {
            var input = state as object[];
            var pheno = input[0] as IPhenotype;
            var countdownEvent = input[1] as CountdownEvent;

            pheno.Evaluate();
            PhenotypeEvaluatedEvent?.Invoke(pheno);

            countdownEvent.Signal();
        }

        public void Serialize(string filename)
        {
            var stream = File.OpenWrite(filename);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Close();
        }

        public void Deserialize(string filename)
        {
            var stream = File.OpenRead(filename);
            var formatter = new BinaryFormatter();
            var ea = (EA)formatter.Deserialize(stream);
            stream.Close();
            this.Population = ea.Population;
            this.PopulationStatistics = ea.PopulationStatistics;
            this.ParentSelection = ea.ParentSelection;
            this.AdultSelection = ea.AdultSelection;
            this.Best = ea.Best;
            this.EAConfiguration = ea.EAConfiguration;
            this.Elites = ea.Elites;
            this.Generation = ea.Generation;
            this.GenerationalBest = ea.GenerationalBest;
            this.RNG = ea.RNG;
            this._offspring = ea._offspring;
            this._offspringSize = ea._offspringSize;
            _newRun = false;
        }

        public void SaveState(string filename)
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented,
                ContractResolver = new AllFieldsContractResolver()
            };
            var json = JsonConvert.SerializeObject(this, jsonSerializerSettings);
            File.WriteAllText(filename, json);
        }

        public void LoadState(string filename)
        {
            var json = File.ReadAllText(filename);
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented,
                ContractResolver = new AllFieldsContractResolver()
            };
            var ea = JsonConvert.DeserializeObject<EA>(json, jsonSerializerSettings);
            this.Population = ea.Population;
            this.PopulationStatistics = ea.PopulationStatistics;
            this.ParentSelection = ea.ParentSelection;
            this.AdultSelection = ea.AdultSelection;
            this.Best = ea.Best;
            this.EAConfiguration = ea.EAConfiguration;
            this.Elites = ea.Elites;
            this.Generation = ea.Generation;
            this.GenerationalBest = ea.GenerationalBest;
            this.RNG = ea.RNG;
            this._offspring = ea._offspring;
            this._offspringSize = ea._offspringSize;
            _newRun = false;
        }

        private class AllFieldsContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                .Select(p => base.CreateProperty(p, memberSerialization))
                                .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                           .Select(f => base.CreateProperty(f, memberSerialization)))
                                .ToList();
                props.ForEach(p => { p.Writable = true; p.Readable = true; });
                return props;
            }
        }

        private class EAConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                var json = JObject.Load(reader);

                var population = new Population(int.Parse(json.GetValue("Size").ToString()));
                var phenos = (JArray)json.GetValue("Phenotypes");

                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                foreach (var pheno in phenos)
                {
                    var p = (IPhenotype)JsonConvert.DeserializeObject(pheno.ToString(), settings);
                    population.Add(p);
                }

                return population;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null)
                    return;

                Population pop = (Population)value;

                var popobj = new JObject();
                popobj.Add("Size", pop.Size);

                var phenos = new JArray();
                foreach (var pheno in pop)
                {
                    phenos.Add(JToken.FromObject(pheno, serializer));
                }
                popobj.Add("Phenotypes", phenos);

                serializer.Serialize(writer, popobj);
            }
        }

    }

}
