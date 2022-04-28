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
using System.Runtime.InteropServices;

namespace Marus.NoiseDistributions
 {

 
    /// <summary>
    /// Struct holding compiled getter and setter
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct NoiseParameterAccessor
    {
        [FieldOffset(0)]
        public Func<INoise, int> IntGetter;
        [FieldOffset(0)]
        public Func<INoise, float> FloatGetter;
        [FieldOffset(0)]
        public Func<INoise, string> StringGetter;

        [FieldOffset(8)]
        public Action<INoise, int> IntSetter;
        [FieldOffset(8)]
        public Action<INoise, float> FloatSetter;
        [FieldOffset(8)]
        public Action<INoise, string> StringSetter;

        [FieldOffset(16)]
        public Type ParameterType;


        static readonly Type _intType = typeof(int);
        static readonly Type _floatType = typeof(float);
        static readonly Type _stringType = typeof(string);


        public string Get(INoise obj)
        {
            if (ParameterType == _intType)
            {
                return IntGetter(obj).ToString();
            }
            else if (ParameterType == _floatType)
            {
                return FloatGetter(obj).ToString();
            }
            else if (ParameterType == _stringType)
            {
                return StringGetter(obj);
            }
            else
            {
                throw new Exception("Getter type not supported!");
            }
        }

        public void Set(INoise obj, string value)
        {
            if (ParameterType == _intType)
            {
                IntSetter(obj, int.Parse(value));
            }
            else if (ParameterType == _floatType)
            {
                FloatSetter(obj, float.Parse(value));
            }
            else if (ParameterType == _stringType)
            {
                StringSetter(obj, value);
            }
            else
            {
                throw new Exception("Setter type not supported!");
            }
        }

    }

 }