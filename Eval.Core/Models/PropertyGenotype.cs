#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Eval.Core.Util.EARandom;

namespace Eval.Core.Models
{
    public abstract class PropertyGenotype<PGType> : Genotype
        where PGType : PropertyGenotype<PGType>
    {
        private readonly PropertyGenotypeElement[] _properties;

        public PropertyGenotypeElement this[int key]
        {
            get => _properties[key];
            private set => _properties[key] = value;
        }

        public PropertyGenotype(Func<Builder, Builder> builder)
            : this(builder(new Builder()).Build())
        {
        }

        public PropertyGenotype(PropertyGenotypeElement[] properties)
        {
            _properties = properties;
        }

        public void Randomize(IRandomNumberGenerator random)
        {
            Mutate(1.0, random);
        }

        public override IGenotype Clone()
        {
            return (PGType)MemberwiseClone();
        }

        public override IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random)
        {
            var otherParams = other as PropertyGenotype<PGType>;
            if (otherParams == null)
            {
                throw new ArgumentException();
            }
            if (_properties.Length != otherParams._properties.Length)
            {
                throw new ArgumentException();
            }

            if (crossover == CrossoverType.OnePoint)
            {
                var length = _properties.Length;
                var splitIndex = random.Next(length + 1);
                var newGeno = (PGType)Clone();

                for (int i = splitIndex; i < length; i++)
                {
                    newGeno[i].TransferValue(otherParams, newGeno);
                }

                return newGeno;
            }
            if (crossover == CrossoverType.Uniform)
            {
                var length = _properties.Length;
                var newGeno = (PGType)Clone();

                for (int i = 0; i < length; i++)
                {
                    newGeno[i].TransferValue(random.NextBool() ? this : otherParams, newGeno);
                }

                return newGeno;
            }
            throw new NotImplementedException(crossover.ToString());
        }

        public override void Mutate(double probability, IRandomNumberGenerator random)
        {
            foreach (var property in _properties)
            {
                property.Mutate(this, probability, random);
            }
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<PropertyGenotypeElement[]>.Default.GetHashCode(_properties);
        }

        public void SetPropertyBounds<T>(Expression<Func<PGType, T>> propertySelector, T minValue, T maxValue)
            where T : struct, IComparable<T>
        {
            var propertyInfo = GetPropertyInfo(propertySelector);
            var property = _properties.FirstOrDefault(p => p.PropertyInfo == propertyInfo) as Property<T>;
            if (property == null)
            {
                throw new ArgumentException($"Property \"{propertyInfo.Name}\" is not a defined parameter in {GetType().Name}. Add it with 'builder.DefineParameter({propertySelector.ToString()}, {minValue}, {maxValue})'");
            }
            property.MinValue = minValue;
            property.MaxValue = maxValue;
        }

        private static PropertyInfo GetPropertyInfo<TIn, TOut>(Expression<Func<TIn, TOut>> selector)
        {
            var memberExpression = selector.Body as MemberExpression;
            return memberExpression.Member as PropertyInfo;
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            foreach (var property in _properties)
            {
                str.AppendLine($"{property.PropertyInfo.Name}: {property.Value(this)}");
            }
            return str.ToString();
        }

        public class Builder
        {
            private readonly IList<PropertyGenotypeElement> _properties;

            public Builder()
            {
                _properties = new List<PropertyGenotypeElement>();
            }

            public PropertyGenotypeElement[] Build()
            {
                return _properties.ToArray();
            }

            public Builder DefineParameter<T>(Expression<Func<PGType, T>> propertySelector, T minValue, T maxValue)
                where T : struct, IComparable<T>
            {
                // TODO: check for duplicates
                var propertyInfo = GetPropertyInfo(propertySelector);
                var property = new Property<T>(propertyInfo, minValue, maxValue);
                _properties.Add(property);
                return this;
            }

            public Builder DefineParameter<T>(Expression<Func<PGType, T>> propertySelector)
            {
                // TODO: check for duplicates
                var propertyInfo = GetPropertyInfo(propertySelector);
                PropertyGenotypeElement property = default;

                if (typeof(T) == typeof(bool))
                {
                    property = new Property<bool>(propertyInfo, false, true);
                }

                if (typeof(T).IsEnum)
                {
                    var enumValues = Enum.GetValues(typeof(T));
                    property = new Property<int>(propertyInfo, 0, enumValues.Length - 1);
                }

                if (property != default)
                {
                    _properties.Add(property);
                    return this;
                }
                
                throw new ArgumentException($"Selected property is not an enum-type: {propertySelector.ToString()}");
            }
        }
    }

    public abstract class PropertyGenotypeElement
    {
        public PropertyInfo PropertyInfo { get; }

        public PropertyGenotypeElement(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public object Clone()
        {
            return this;
        }

        public abstract void TransferValue(object source, object target);
        public abstract void Mutate(object target, double factor, IRandomNumberGenerator random);
        public abstract object Value(object target);
    }

    class Property<T> : PropertyGenotypeElement
        where T : struct, IComparable<T>
    {
        public T MinValue { get; set; }
        public T MaxValue { get; set; }

        public Property(PropertyInfo propertyInfo, T minValue, T maxValue)
            : base(propertyInfo)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public T GetValue(object target)
        {
            return (T)PropertyInfo.GetValue(target);
        }

        public override object Value(object target)
        {
            return PropertyInfo.GetValue(target);
        }

        public void SetValue(object target, T value)
        {
            if (MinValue.CompareTo(value) == 1)
            {
                value = MinValue;
            }
            else if (MaxValue.CompareTo(value) == -1)
            {
                value = MaxValue;
            }
            PropertyInfo.SetValue(target, value);
        }

        public override void Mutate(object target, double factor, IRandomNumberGenerator random)
        {
            if (random.NextDouble() < factor)
            {
                object newValue = default;

                if (typeof(T) == typeof(int)) newValue = random.Next(MinValue as int? ?? 0, (MaxValue as int?) + 1 ?? 1);
                else if (typeof(T) == typeof(double)) newValue = random.NextDouble(MinValue as double? ?? 0, MaxValue as double? ?? 1);
                else if (typeof(T) == typeof(float)) newValue = (float)random.NextDouble(MinValue as float? ?? 0, MaxValue as float? ?? 1);
                else if (typeof(T) == typeof(bool)) newValue = random.NextBool();

                SetValue(target, (T)newValue);
            }
        }

        public override void TransferValue(object source, object target)
        {
            SetValue(target, GetValue(source));
        }
    }
}
