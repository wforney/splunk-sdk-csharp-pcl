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

//// TODO: Ensure this code is solid
//// [ ] Support more than one level of inheritance => move away from generic implementation.
//// [O] Contracts
//// [O] Documentation

namespace Splunk.Client
{
    public abstract partial class Args<TArgs> where TArgs : Args<TArgs>
    {
        /// <summary>
        /// An ordinal.
        /// </summary>
        /// <seealso cref="T:System.Collections.Generic.IEnumerable{Splunk.Client.Argument}"/>
        private readonly record struct Ordinal : IComparable<Ordinal>, IEquatable<Ordinal>
        {
            public Ordinal(int position, string name)
            {
                this.Position = position;
                this.Name = name;
            }

            public int Position { get; init; }

            public string Name { get; init; }

            public int CompareTo(Ordinal other)
            {
                var result = this.Position - other.Position;
                return result != 0 ? result : string.Compare(this.Name, other.Name, StringComparison.Ordinal);
            }

            /// <inheritdoc />
            public override string ToString() => $"{this.Position}, {this.Name}";
        }
    }
}
