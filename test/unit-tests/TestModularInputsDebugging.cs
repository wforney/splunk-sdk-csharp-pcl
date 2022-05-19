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

namespace Splunk.Client.UnitTests;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Splunk.ModularInputs;
using Xunit;

public class TestModularInputsDebugging
{
    private readonly TextReader _stdin;
    private readonly TextWriter _stdout;
    private readonly TextWriter _stderr;

    public TestModularInputsDebugging()
    {
        this._stdin = new StringReader(@"<?xml version=""1.0"" encoding=""utf-16""?>
                <input xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                    <server_host>tiny</server_host>
                    <server_uri>https://127.0.0.1:8089</server_uri>
                    <checkpoint_dir>/some/dir</checkpoint_dir>
                    <session_key>123102983109283019283</session_key>
                    <configuration>
                        <stanza name=""random_numbers://aaa"">
                            <param name=""min"">0</param>
                            <param name=""max"">5</param>
                        </stanza>
                    </configuration>
                </input>");
        this._stdout = new StringWriter();
        this._stderr = new StringWriter();
        ModularInput._stdin = this._stdin;
        ModularInput._stdout = this._stdout;
        ModularInput._stderr = this._stderr;
    }

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public void ShouldWaitUntilTimeout()
    {
        ModularInput._isAttached = () => false;
        var start = DateTime.Now;
        ModularInput.WaitForAttach(5);
        var end = DateTime.Now;
        Assert.True((end - start).Seconds >= 5);
    }

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public void ShouldWaitUntilAttach()
    {
        ModularInput._isAttached = () => false;
        var start = DateTime.Now;
        var exited = false;

        var thread = new Thread(() =>
        {
            ModularInput.WaitForAttach(10);
            exited = true;
        });

        thread.Start();

        ModularInput._isAttached = () => true;
        Thread.Sleep(1000);
        var end = DateTime.Now;
        Assert.True((end - start).Seconds <= 2);
        Assert.True(exited);
    }

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public void ShoulldWaitForAttachReturnsTrueWhenValidateArgumentsArgIsPassed()
    {
        var args = new[] { "--validate-arguments" };
        var shouldWait = ModularInput.ShouldWaitForDebuggerToAttach(args, DebuggerAttachPoints.ValidateArguments);
        Assert.True(shouldWait);
    }

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public void ShouldWaitForDebuggerToAttachReturnsTrueWhenNoArgIsPassed()
    {
        var args = new string[0];
        var shouldWait = ModularInput.ShouldWaitForDebuggerToAttach(args, DebuggerAttachPoints.StreamEvents);
        Assert.True(shouldWait);
    }

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public void ShouldWaitForDebuggerToAttachReturnsFalseWhenNoneIsPassed()
    {
        var args = new string[] { "--scheme" };
        var shouldWait = ModularInput.ShouldWaitForDebuggerToAttach(args, DebuggerAttachPoints.None);
        Assert.False(shouldWait);

        args = new string[] { "--validate-arguments" };
        shouldWait = ModularInput.ShouldWaitForDebuggerToAttach(args, DebuggerAttachPoints.None);
        Assert.False(shouldWait);

        args = new string[0];
        shouldWait = ModularInput.ShouldWaitForDebuggerToAttach(args, DebuggerAttachPoints.None);
        Assert.False(shouldWait);
    }

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public void ShouldThrowWhenTimeoutIsZeroAndAttachPointsAreSet() => Assert.Throws<ArgumentOutOfRangeException>(() => ModularInput.Run<TestDebugInput>(new string[0], DebuggerAttachPoints.StreamEvents, 0));

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public async Task ShouldWaitForAttachWhenInputIsRanAndAttachPointIsSpecified()
    {
        var checkedIsAttached = false;
        ModularInput._isAttached = () =>
        {
            checkedIsAttached = true;
            return checkedIsAttached;
        };

        var input = new TestDebugInput();
        _ = await input.RunAsync(new string[0], this._stdin, this._stdout, this._stderr,
            attachPoints: DebuggerAttachPoints.StreamEvents,
            timeout: 1);
        Assert.True(checkedIsAttached);
    }

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public void ShouldInvokeRunAsyncWhenRunIsCalled()
    {
        _ = ModularInput.Run<TestDebugInput>(new string[0]);
        Assert.True(TestDebugInput.Executed);
    }

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public void ShouldThrowExceptionWhenRunIsCalledIfTimeIsZeroAndAttachPointsWereSet() => Assert.Throws<ArgumentOutOfRangeException>(() => ModularInput.Run<TestDebugInput>(new string[0], DebuggerAttachPoints.StreamEvents, 0));

    [Trait("unit-test", "Splunk.ModularInputs.ModularInput")]
    [Fact]
    public void ShouldNotThrowExceptionWhenRunIsCalledIfTimeIsZeroAndAttachPointsWereNotSet() => ModularInput.Run<TestDebugInput>(new string[0], DebuggerAttachPoints.None, 0);

    public class TestDebugInput : ModularInput
    {
        public static bool Executed = false;

        public override Scheme Scheme => throw new NotImplementedException();

        public override Task StreamEventsAsync(InputDefinition inputDefinition, EventWriter eventWriter)
        {
            Executed = true;
            return Task.FromResult(false);
        }
    }
}
