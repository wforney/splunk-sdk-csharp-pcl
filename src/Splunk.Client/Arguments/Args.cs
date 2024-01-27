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

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Splunk.Client.Arguments;

//// TODO: Ensure this code is solid
//// [ ] Support more than one level of inheritance => move away from generic implementation.
//// [O] Contracts
//// [O] Documentation

namespace Splunk.Client
{
    /// <summary>
    /// Provides a base class for representing strongly typed arguments to Splunk
    /// endpoints.
    /// </summary>
    /// <typeparam name="TArgs">
    /// Type of the arguments.
    /// </typeparam>
    /// <seealso cref="T:System.Collections.Generic.IEnumerable{Splunk.Client.Argument}"/>
    public abstract partial class Args<TArgs> : IEnumerable<Argument> where TArgs : Args<TArgs>
    {
        static Args()
        {
            var propertyFormatters = new Dictionary<Type, Formatter>()
            {
                { typeof(bool),     new Formatter { Format = FormatBoolean } },
                { typeof(bool?),    new Formatter { Format = FormatBoolean } },
                { typeof(byte),     new Formatter { Format = FormatNumber  } },
                { typeof(byte?),    new Formatter { Format = FormatNumber  } },
                { typeof(sbyte),    new Formatter { Format = FormatNumber  } },
                { typeof(sbyte?),   new Formatter { Format = FormatNumber  } },
                { typeof(short),    new Formatter { Format = FormatNumber  } },
                { typeof(short?),   new Formatter { Format = FormatNumber  } },
                { typeof(ushort),   new Formatter { Format = FormatNumber  } },
                { typeof(ushort?),  new Formatter { Format = FormatNumber  } },
                { typeof(int),      new Formatter { Format = FormatNumber  } },
                { typeof(int?),     new Formatter { Format = FormatNumber  } },
                { typeof(uint),     new Formatter { Format = FormatNumber  } },
                { typeof(uint?),    new Formatter { Format = FormatNumber  } },
                { typeof(long),     new Formatter { Format = FormatNumber  } },
                { typeof(long?),    new Formatter { Format = FormatNumber  } },
                { typeof(ulong),    new Formatter { Format = FormatNumber  } },
                { typeof(ulong?),   new Formatter { Format = FormatNumber  } },
                { typeof(float),    new Formatter { Format = FormatNumber  } },
                { typeof(float?),   new Formatter { Format = FormatNumber  } },
                { typeof(double),   new Formatter { Format = FormatNumber  } },
                { typeof(double?),  new Formatter { Format = FormatNumber  } },
                { typeof(decimal),  new Formatter { Format = FormatNumber  } },
                { typeof(decimal?), new Formatter { Format = FormatNumber  } },
                { typeof(string),   new Formatter { Format = FormatString  } }
            };

            var defaultFormatter = new Formatter { Format = FormatString };
            var parameters = new SortedSet<Parameter>();

            foreach (var propertyInfo in typeof(TArgs).GetRuntimeProperties())
            {
                var dataMember = propertyInfo.GetCustomAttribute<DataMemberAttribute>();

                if (dataMember is null)
                {
                    var text = string.Format(CultureInfo.CurrentCulture, "Missing DataMemberAttribute on {0}.{1}", propertyInfo.PropertyType.Name, propertyInfo.Name);
                    throw new InvalidDataContractException(text);
                }

                var propertyName = propertyInfo.Name;
                var propertyType = propertyInfo.PropertyType;
                var propertyTypeInfo = propertyType.GetTypeInfo();

                var formatter = GetPropertyFormatter(propertyName, null, propertyType, propertyTypeInfo, propertyFormatters);

                if (formatter is null)
                {
                    var interfaces = propertyType.GetTypeInfo().ImplementedInterfaces;
                    var isCollection = false;

                    formatter = defaultFormatter;

                    foreach (var @interface in interfaces)
                    {
                        if (@interface.IsConstructedGenericType)
                        {
                            if (@interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            {
                                //// IEnumerable<T> implements IEnumerable => we are good to go

                                var itemType = @interface.GenericTypeArguments[0];

                                formatter = GetPropertyFormatter(propertyName, propertyType, itemType, itemType.GetTypeInfo(), propertyFormatters);
                                isCollection = true;

                                break;
                            }
                        }
                        else if (@interface == typeof(IEnumerable))
                        {
                            //// Keep looking because we'd prefer to use a more specific formatter
                            isCollection = true;
                        }
                    }

                    formatter = new Formatter { Format = formatter.Value.Format, IsCollection = isCollection };
                }

                var defaultValue = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();

                _ = parameters.Add(new Parameter(dataMember.Name, dataMember.Order, propertyInfo)
                {
                    // Properties

                    DefaultValue = defaultValue?.Value,
                    EmitDefaultValue = dataMember.EmitDefaultValue,
                    IsCollection = formatter.Value.IsCollection,
                    IsRequired = dataMember.IsRequired,

                    // Methods

                    Format = (formatter ?? defaultFormatter).Format
                });
            }

            Parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Args&lt;TArgs&gt;"/> class.
        /// </summary>
        protected Args()
        {
            foreach (var serializationEntry in Parameters.Where(entry => entry.DefaultValue != null))
            {
                serializationEntry.SetValue(this, serializationEntry.DefaultValue);
            }
        }

        /// <summary>
        /// Gets an enumerator that produces an <see cref="Argument"/> sequence
        /// based on the serialization attributes of the properties of the 
        /// current <see cref="Args&lt;TArgs&gt;"/> instance.
        /// </summary>
        /// <exception cref="SerializationException">
        /// Thrown when a Serialization error condition occurs.
        /// </exception>
        /// <returns>
        /// An object for enumerating the <see cref="Argument"/> sequence.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Argument>)this).GetEnumerator();

        /// <summary>
        /// Gets an enumerator that produces an <see cref="Argument"/> sequence
        /// based on the serialization attributes of the properties of the 
        /// current <see cref="Args&lt;TArgs&gt;"/> instance.
        /// </summary>
        /// <exception cref="SerializationException">
        /// Thrown when a Serialization error condition occurs.
        /// </exception>
        /// <returns>
        /// An object for enumerating the <see cref="Argument"/> sequence.
        /// </returns>
        IEnumerator<Argument> IEnumerable<Argument>.GetEnumerator()
        {
            foreach (var parameter in Args<TArgs>.Parameters)
            {
                var value = parameter.GetValue(this);

                if (value is null)
                {
                    if (parameter.IsRequired)
                    {
                        throw new SerializationException(string.Format("Missing value for required parameter {0}", parameter.Name));
                    }
                    continue;
                }

                if (!parameter.EmitDefaultValue && value.Equals(parameter.DefaultValue))
                {
                    continue;
                }

                if (!parameter.IsCollection)
                {
                    yield return new Argument(parameter.Name, parameter.Format(value));
                    continue;
                }

                foreach (var item in (IEnumerable)value)
                {
                    yield return new Argument(parameter.Name, parameter.Format(item));
                }
            }
        }

        /// <summary>
        /// Gets a string representation for the current
        /// <see cref="Args&lt;TArgs&gt;"/>.
        /// </summary>
        /// <returns>
        /// A string representation of the current <see cref="Args&lt;TArgs&gt;"/>.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            void append(object item, string name, Func<object, string> format)
            {
                _ = builder.Append(name);
                _ = builder.Append('=');
                _ = builder.Append(format(item));
                _ = builder.Append("; ");
            }

            foreach (var parameter in Args<TArgs>.Parameters)
            {
                var value = parameter.GetValue(this);

                if (value is null)
                {
                    append("null", parameter.Name, FormatString);
                    continue;
                }

                if (!parameter.IsCollection)
                {
                    append(value, parameter.Name, parameter.Format);
                    continue;
                }

                foreach (var item in (IEnumerable)value)
                {
                    append(item, parameter.Name, parameter.Format);
                }
            }

            if (builder.Length > 0)
            {
                builder.Length -= 2;
            }

            return builder.ToString();
        }

        private static readonly SortedSet<Parameter> Parameters;

        private static string FormatBoolean(object? value) => ((bool?)value ?? false) ? "1" : "0";

        private static string? FormatNumber(object? value) => value?.ToString();

        private static string? FormatString(object? value) => value?.ToString();

        private static Formatter? GetPropertyFormatter(string propertyName, Type? container, Type type, TypeInfo info, Dictionary<Type, Formatter> formatters)
        {

            if (formatters.TryGetValue(container ?? type, out var formatter))
            {
                return formatter;
            }

            if (container != null && formatters.TryGetValue(type, out formatter))
            {
                formatter.IsCollection = true;
            }
            else
            {
                var enumType = GetEnum(type, info);

                if (enumType != null)
                {
                    //// TODO: Add two items to formatters for each enum: 
                    //// formatters.Add(enumType, formatter)
                    //// formatters.Add(info.IsEnum ? typeof(Nullable<>).MakeGenericType(type), formatter)

                    var map = new Dictionary<int, string>();

                    foreach (var value in Enum.GetValues(enumType))
                    {
                        var name = Enum.GetName(enumType, value);
                        var field = name is null ? null : enumType.GetRuntimeField(name);
                        var enumMember = field?.GetCustomAttribute<EnumMemberAttribute>();

                        map[(int)value] = enumMember is null ? name : enumMember.Value;
                    }

                    string format(object value)
                    {

                        if (map.TryGetValue((int)value, out var name))
                        {
                            return name;
                        }

                        var text = string.Format(CultureInfo.CurrentCulture, "{0}.{1}: {2}",
                            typeof(TArgs).Name, propertyName, value);

                        throw new ArgumentException(text);
                    }

                    formatter = new Formatter
                    {
                        Format = format,
                        IsCollection = container != null
                    };
                }
                else if (container != null)
                {
                    formatter = new Formatter { Format = FormatString, IsCollection = true };
                }
                else
                {
                    return null;
                }
            }

            formatters.Add(container ?? type, formatter);
            return formatter;
        }

        private static Type? GetEnum(Type type, TypeInfo info)
        {
            if (info.IsEnum)
            {
                return type;
            }

            var t = type is null ? null : Nullable.GetUnderlyingType(type);

            if (t is null)
            {
                return null;
            }

            info = t.GetTypeInfo();
            return info.IsEnum ? type : null;
        }
    }
}
