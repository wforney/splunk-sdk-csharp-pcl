// <copyright file="MockContext.cs" company="Splunk, Inc.">
//     Copyright 2014 Splunk, Inc.
// </copyright>
/*
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

using System.Configuration;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using Splunk.Client.Helpers;

namespace Splunk.Client.Helper;

/// <summary>
/// Provides a class for faking HTTP requests and responses from a Splunk server.
/// </summary>
public partial class MockContext : Context
{
    private static Func<object?, object?>? getOrElse;

    private static Session? session;

    /// <summary>
    /// Initializes static members of the <see cref="MockContext"/> class.
    /// </summary>
    static MockContext()
    {
        string? setting;

        setting = ConfigurationManager.AppSettings["MockContext.RecordingDirectoryName"];
        RecordingDirectoryName = setting ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Recordings");

        setting = ConfigurationManager.AppSettings["MockContext.Mode"];
        Mode = setting is null ? MockContextMode.Run : (MockContextMode)Enum.Parse(typeof(MockContextMode), setting);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockContext" /> class with a protocol, host,
    /// port number.
    /// </summary>
    /// <param name="protocol">
    /// The <see typeref="Protocol" /> used to communiate with <see typeref="Host" />.
    /// </param>
    /// <param name="host">The DNS name of a Splunk server instance.</param>
    /// <param name="port">The port number used to communicate with <see typeref="Host" />.</param>
    /// <param name="timeout">The timeout.</param>
    /// <remarks>
    /// <para><b>References</b></para>
    /// <list type="number">
    /// <item>
    /// <description>
    /// <a href="http://goo.gl/ppbIlm">How to Avoid Creating Real Tasks When Unit Testing Async</a>.
    /// </description>
    /// </item>
    /// <item>
    /// <description><a href="http://goo.gl/YUFhAO">ObjectContent Class</a>.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public MockContext(Scheme protocol, string host, int port, TimeSpan timeout = default)
        : base(protocol, host, port, timeout, CreateMessageHandler())
    {
    }

    /// <summary>
    /// Gets the caller identifier.
    /// </summary>
    /// <value>The caller identifier.</value>
    public static string? CallerId => session?.Name;

    /// <summary>
    /// Gets the mode.
    /// </summary>
    /// <value>The mode.</value>
    public static MockContextMode Mode { get; private set; }

    /// <summary>
    /// Gets the name of the recording directory.
    /// </summary>
    /// <value>The name of the recording directory.</value>
    public static string RecordingDirectoryName { get; private set; }

    /// <summary>
    /// Gets the recording filename.
    /// </summary>
    /// <value>The recording filename.</value>
    public static string? RecordingFilename { get; private set; }

    /// <summary>
    /// Begins the specified caller identifier.
    /// </summary>
    /// <param name="callerId">The caller identifier.</param>
    /// <exception cref="System.InvalidOperationException"></exception>
    public static void Begin(string callerId)
    {
        ArgumentNullException.ThrowIfNull(callerId);

        RecordingFilename = Path.Combine(RecordingDirectoryName, string.Join(".", callerId, "json", "gz"));

        switch (Mode)
        {
            case MockContextMode.Run:
                session = new Session(callerId);
                getOrElse = Noop;
                break;

            case MockContextMode.Record:
                session = new Session(callerId);
                getOrElse = Enqueue;
                break;

            case MockContextMode.Playback:

                //var serializer = new DataContractJsonSerializer(typeof(Session), null, int.MaxValue, false, new JsonDataContractSurrogate(), false);

                using (var stream = new FileStream(RecordingFilename, FileMode.Open))
                {
                    using var compressedStream = new GZipStream(stream, CompressionMode.Decompress);
                    session = System.Text.Json.JsonSerializer.Deserialize<Session>(compressedStream);
                }

                getOrElse = Dequeue;
                break;

            default: throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Ends this instance.
    /// </summary>
    /// <exception cref="System.InvalidOperationException"></exception>
    public static void End()
    {
        switch (Mode)
        {
            case MockContextMode.Run:
                break;

            case MockContextMode.Record:
                if (RecordingFilename is null)
                {
                    throw new InvalidOperationException("RecordingFilename is null.");
                }

                var serializer = new DataContractJsonSerializer(typeof(Session));

                _ = Directory.CreateDirectory(RecordingDirectoryName);
                session?.Data.TrimExcess();
                session?.Recordings.TrimExcess();

                using (var stream = new FileStream(RecordingFilename, FileMode.Create))
                {
                    using var compressedStream = new GZipStream(stream, CompressionLevel.Optimal);
                    serializer.WriteObject(compressedStream, session);
                }

                break;

            case MockContextMode.Playback:

                Debug.Assert(session?.Data.Count == 0);
                Debug.Assert(session.Recordings.Count == 0);
                break;

            default: throw new InvalidOperationException();
        }

        session = null;
    }

    /// <summary>
    /// Gets the or else.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <returns>T.</returns>
    public static T? GetOrElse<T>(T value)
    {
        var o = getOrElse?.Invoke(value);
        return (T?)o;
    }

    private static HttpMessageHandler? CreateMessageHandler() => Mode switch
    {
        MockContextMode.Run => null,
        MockContextMode.Record => new Recorder(),
        MockContextMode.Playback => new Player(),
        _ => throw new InvalidOperationException(),
    };

    private static object? Dequeue(object? o) => session?.Data.Dequeue();

    private static object? Enqueue(object? o)
    {
        //// DateTime values are serialized with millisecond precision, not
        //// at the precision of a tick, hence we must compensate.

        var dt = o as DateTime?;

        if (dt.HasValue)
        {
            o = dt.Value.AddTicks(-(dt.Value.Ticks % TimeSpan.TicksPerSecond));
        }

        if (o is not null)
        {
            session?.Data.Enqueue(o);
        }

        return o;
    }

    private static object? Noop(object? o) => o;

    /*
    /// <summary>
    /// Class JsonDataContractSurrogate. Implements the <see cref="IDataContractSurrogate" />
    /// </summary>
    /// <seealso cref="IDataContractSurrogate" />
    private class JsonDataContractSurrogate : IDataContractSurrogate
    {
        private static readonly Regex DateTimeFormat = new(@"/Date\((\d+)([+-]\d+)\)/");

        private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Gets the custom data to export.
        /// </summary>
        /// <param name="clrType">Type of the color.</param>
        /// <param name="dataContractType">Type of the data contract.</param>
        /// <returns>System.Object.</returns>
        public object? GetCustomDataToExport(Type clrType, Type dataContractType) => null; // unused

        /// <summary>
        /// Gets the custom data to export.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="dataContractType">Type of the data contract.</param>
        /// <returns>System.Object.</returns>
        public object? GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, Type dataContractType) => null; // unused

        /// <summary>
        /// Gets the type of the data contract.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        public Type? GetDataContractType(Type type) => null; // unused

        /// <summary>
        /// Gets the deserialized object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns>System.Object.</returns>
        public object GetDeserializedObject(object obj, Type targetType)
        {
            if (obj is not Queue<object> data)
            {
                return obj;
            }

            if (data.Count == 0)
            {
                return obj;
            }

            var replacement = new Queue<object>(data.Count);

            foreach (var item in data)
            {
                var enqueuedItem = item;

                if (item is string s)
                {
                    var match = DateTimeFormat.Match(s);

                    if (match.Success)
                    {
                        var timeZoneDesignator = short.Parse(match.Groups[2].Value);
                        var milliseconds = long.Parse(match.Groups[1].Value);
                        var dateTime = Epoch.Add(TimeSpan.FromMilliseconds(milliseconds));

                        dateTime = dateTime.AddHours(timeZoneDesignator / 100);
                        dateTime = dateTime.AddMinutes(timeZoneDesignator % 100);

                        enqueuedItem = dateTime;
                    }
                }

                replacement.Enqueue(enqueuedItem);
            }

            return replacement;
        }

        /// <summary>
        /// Gets the known custom data types.
        /// </summary>
        /// <param name="customDataTypes">The custom data types.</param>
        public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> customDataTypes)
        {
            // unused
        }

        /// <summary>
        /// Gets the object to serialize.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns>System.Object.</returns>
        public object GetObjectToSerialize(object obj, Type targetType) => obj; // unused

        /// <summary>
        /// Gets the referenced type on import.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeNamespace">The type namespace.</param>
        /// <param name="customData">The custom data.</param>
        /// <returns>Type.</returns>
        public Type? GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData) => null; // unused

        /// <summary>
        /// Processes the type of the imported.
        /// </summary>
        /// <param name="typeDeclaration">The type declaration.</param>
        /// <param name="compileUnit">The compile unit.</param>
        /// <returns>CodeTypeDeclaration.</returns>
        public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit) => typeDeclaration; // unused
    }
    */
}
