/*
 * Copyright 2014 Splunk, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"): you may
 * not use this file except in compliance with the License. You may obtain
 * a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

using System.Reflection;

//// TODO: Ensure this code is solid
//// [ ] Support more than one level of inheritance => move away from generic implementation.
//// [O] Contracts
//// [O] Documentation

namespace Splunk.Client
{
    public abstract partial class Args<TArgs> where TArgs : Args<TArgs>
    {
        private class Parameter : IComparable, IComparable<Parameter>, IEquatable<Parameter>
        {
            public Parameter(string name, int position, PropertyInfo propertyInfo)
            {
                this.ordinal = new Ordinal(position, name);
                this.propertyInfo = propertyInfo;
            }

            public object? DefaultValue { get; set; }

            public bool EmitDefaultValue { get; set; }

            public bool IsCollection { get; set; }

            public bool IsRequired { get; set; }

            public string Name => this.ordinal.Name;

            public int Position => this.ordinal.Position;

            public int CompareTo(object? other) => this.CompareTo(other as Parameter);

            public int CompareTo(Parameter? other) => other is null ? 1 : ReferenceEquals(this, other) ? 0 : this.ordinal.CompareTo(other.ordinal);

            public override bool Equals(object? other) => this.Equals(other as Parameter);

            public bool Equals(Parameter? other) => other is not null && (ReferenceEquals(this, other) || this.ordinal.Equals(other.ordinal));

            public Func<object?, string?>? Format { get; set; }

            public override int GetHashCode() => this.ordinal.GetHashCode();

            public object? GetValue(object o) => this.propertyInfo.GetValue(o);

            public void SetValue(object o, object value) => this.propertyInfo.SetValue(o, value);

            private readonly PropertyInfo propertyInfo;
            private readonly Ordinal ordinal;
        }
    }
}
