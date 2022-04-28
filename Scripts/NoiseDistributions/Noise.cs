// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Marus.NoiseDistributions
{
    /// <summary>
    /// Main static class for nose sampling
    /// </summary>
    public static class Noise
    {

        private static List<Type> _noiseTypes;

        /// <summary>
        /// One instance of the noise class per noise type 
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="INoise"></typeparam>
        /// <returns></returns>
        private static Dictionary<string, INoise> _noiseInstances
            = new Dictionary<string, INoise>();

        /// <summary>
        /// Compiled getters and setters for noise instances 
        /// </summary>
        /// <param name="_noiseParameterAccessors"></param>
        /// <typeparam name="(string"></typeparam>
        /// <typeparam name="string)"></typeparam>
        /// <typeparam name="NoiseParameterAccessor"></typeparam>
        private static Dictionary<(string, string), NoiseParameterAccessor> _noiseParameterAccessors
            = new Dictionary<(string, string), NoiseParameterAccessor>();


        /// <summary>
        /// List of allowed types for noise fields 
        /// </summary>
        /// <value></value>
        readonly static List<Type> _allowedTypes = new List<Type>
        {
            typeof(string), typeof(int), typeof(float)
        };

        /// <summary>
        /// Exclude assemblies where noise types are searched
        /// </summary>
        /// <value></value>
        readonly static List<string> _excludeAssemblies = new List<string>
        { 
            "EditMode", "PlayMode", "TestUtils"
        };

        static BindingFlags _fieldFlags = 
            BindingFlags.Instance | BindingFlags.Public;


        /// <summary>
        /// List of all noise types
        /// </summary>
        public static IReadOnlyList<Type> NoiseTypes
        {
            get 
            {
                if (_noiseTypes == null)
                {
                    InitNoiseTypesFromAssemblies();
                }
                return _noiseTypes;
            }
        }

        /// <summary>
        /// Sample from noise distribution defined in the <see cref="NoiseParameters"/> class
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static float Sample(in NoiseParameters parameters)
        {
            var instance = GetNoiseInstance(parameters.NoiseTypeFullName);
            if (instance == null)
            {
                Debug.Log($"Noise instance with assembly name {parameters.NoiseTypeFullName} does not exist");
                return 0;
            }
            SetInstanceParams(instance, in parameters);
            return instance.Sample();
        }


        private static void InitNoiseTypesFromAssemblies()
        {
            var appDomain = AppDomain.CurrentDomain;

            // hacky removal of test assemblies
            var assemblies = appDomain.GetAssemblies().Where(x =>
                !_excludeAssemblies.Contains(x.GetName().Name));
            _noiseTypes = assemblies.Aggregate(new List<Type>(), (curr, x) => 
            {
                curr.AddRange(
                    x.GetTypes().Where(typ => 
                        typeof(INoise).IsAssignableFrom(typ) 
                        && typ.AssemblyQualifiedName != typeof(INoise).AssemblyQualifiedName
                    )
                );
                return curr;
            });
            _noiseTypes.RemoveAll(x => x.FullName == typeof(NoNoise).FullName);
            _noiseTypes.Insert(0, typeof(NoNoise));
        }

        private static void SetInstanceParams(INoise instance, in NoiseParameters parameters)
        {
            for (var i = 0; i < parameters.ParameterKeys.Count; i++)
            {
                var key = parameters.ParameterKeys[i];
                var value = parameters.ParameterValues[i];
                _noiseParameterAccessors[(parameters.NoiseTypeFullName, key)].Set(instance, value);
            }
        }
        private static INoise GetNoiseInstance(string typeFullName)
        {
            INoise noise;
            if (_noiseInstances.TryGetValue(typeFullName, out noise))
            {
                return noise;
            }
            var typ = _noiseTypes.FirstOrDefault(x => 
                    x.FullName == typeFullName);

            if (typ == null)
            {
                return null;
            }
            noise = (INoise)Activator.CreateInstance(typ);
            _noiseInstances[typeFullName] = noise;
            CompileGettersAndSetters(noise);
            return noise;
        }

        private static void CompileGettersAndSetters(INoise instance)
        {
            var typ = instance.GetType();
            foreach (var f in instance.GetType().GetFields(_fieldFlags))
            {
                var paramAccessor = new NoiseParameterAccessor
                {
                    ParameterType = f.FieldType
                };
                if (f.FieldType == typeof(int))
                {
                    paramAccessor.IntGetter = GetCompiledGenericGetter<INoise, int>(f.Name, typ);
                    paramAccessor.IntSetter = GetCompiledGenericSetter<INoise, int>(f.Name, typ);
                }
                else if (f.FieldType == typeof(float))
                {
                    paramAccessor.FloatGetter = GetCompiledGenericGetter<INoise, float>(f.Name, typ);
                    paramAccessor.FloatSetter = GetCompiledGenericSetter<INoise, float>(f.Name, typ);
                }
                else if (f.FieldType == typeof(string))
                {
                    paramAccessor.StringGetter = GetCompiledGenericGetter<INoise, string>(f.Name, typ);
                    paramAccessor.StringSetter = GetCompiledGenericSetter<INoise, string>(f.Name, typ);
                }
                else
                {
                    throw new Exception($"Unsupported field type in noise definition{f.FieldType.Name}");
                }
                _noiseParameterAccessors.Add((typ.FullName, f.Name), paramAccessor);
            }
        }

        public static bool IsAllowedNoiseType(Type typ)
        {
            return _allowedTypes.Any(x => x.FullName == typ.FullName);
        }

        private static Func<TObject, TProperty> GetCompiledGenericGetter<TObject, TProperty>(string propertyName, Type derivedType=null)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject), "value");
            Expression converted = (derivedType != null) ? Expression.Convert(paramExpression, derivedType) : paramExpression;
            Expression propertyGetterExpression = Expression.Field(converted, propertyName);

            Func<TObject, TProperty> result =
                Expression.Lambda<Func<TObject, TProperty>>(propertyGetterExpression, paramExpression).Compile();

            return result;
        }

        // returns property setter:
        private static Action<TObject, TProperty> GetCompiledGenericSetter<TObject, TProperty>(string propertyName, Type derivedType=null)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject), "value");

            ParameterExpression paramExpression2 = Expression.Parameter(typeof(TProperty), propertyName);

            Expression converted = (derivedType != null) ? Expression.Convert(paramExpression, derivedType) : paramExpression;
            MemberExpression propertyExpression = Expression.Field(converted, propertyName);

            Action<TObject, TProperty> result = Expression.Lambda<Action<TObject, TProperty>>
            (
                Expression.Assign(propertyExpression, paramExpression2), paramExpression, paramExpression2
            ).Compile();

            return result;
        }


        class NoNoise : INoise
        {
            public float Sample()
            {
                return 0;
            }
        }
    }
}