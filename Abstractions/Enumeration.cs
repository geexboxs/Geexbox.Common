using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Geexbox.Common
{
    /// <summary>
    /// simulation type for typescript string enum
    /// </summary>
    [JsonConverter(typeof(EnumerationConverter))]
    [ModelBinder(BinderType = typeof(EnumerationBinder))]
    public abstract class Enumeration : ValueObject
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Name;
        }

        protected Enumeration(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public static implicit operator string(Enumeration value)
        {
            return value?.Value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Name;
            yield return Value;
        }

        public static bool TryParse<TImplemetions>(string value, out TImplemetions result) where TImplemetions : Enumeration
        {
            result = Enumeration.GetAll<TImplemetions>().FirstOrDefault(x => x.Value.CompareTo(value) == 0);
            return result != default(TImplemetions);
        }

        public static TImplemetions Parse<TImplemetions>(string value) where TImplemetions : Enumeration
        {
            var result = Enumeration.GetAll<TImplemetions>().First(x => x.Value.CompareTo(value) == 0);
            return result;
        }

        public static IEnumerable<TImplemention> GetAll<TImplemention>() where TImplemention : Enumeration
        {
            var type = typeof(TImplemention);
            var typeInfo = type.GetTypeInfo();
            var fields = typeInfo.GetFields(BindingFlags.Public |
                                                      BindingFlags.Static |
                                                      BindingFlags.DeclaredOnly);
            foreach (var info in fields)
            {
                if (info.GetValue(default(TImplemention)) is TImplemention locatedValue)
                {
                    yield return locatedValue;
                }
            }
        }

        public override bool Equals(object obj)
        {
            var otherValue = obj as Enumeration;
            if (otherValue == null)
            {
                return false;
            }
            var typeMatches = GetType() == obj.GetType();
            var valueMatches = Value.Equals(otherValue.Value);
            return typeMatches && valueMatches;
        }

        public override int GetHashCode()
        {
            return EqualityComparer<string>.Default.GetHashCode(Value);
        }

        public int CompareTo(object other)
        {
            return Value.CompareTo(((Enumeration)other).Value);
        }

        // Other utility methods ...
    }

    public class EnumerationBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            var locatedValue = typeof(Enumeration).GetMethod(nameof(Enumeration.Parse))
                ?.MakeGenericMethod(bindingContext.ModelType).Invoke(null, new[] { value });

            if (locatedValue == default)
                return Task.CompletedTask;
            bindingContext.Result = ModelBindingResult.Success(locatedValue);
            return Task.CompletedTask;

        }
    }

    public class EnumerationConverter : JsonConverter<Enumeration>
    {
        public override void WriteJson(JsonWriter writer, Enumeration value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }

        //public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        //{
        //    return typeof(Enumeration).MakeGenericType(existingValue?.GetType()).GetMethod(nameof(Enumeration.Parse))
        //        ?.Invoke(null, new[] { existingValue });
        //}

        public override Enumeration ReadJson(JsonReader reader, Type objectType, Enumeration existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            return typeof(Enumeration).GetMethod(nameof(Enumeration.Parse)).MakeGenericMethod(objectType)
                .Invoke(null, new[] { (string)reader.Value }) as Enumeration;
        }
    }
}
