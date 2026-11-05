using System.Text.Json;
using OmniSharp.Extensions.DebugAdapter.Protocol.Events;
using OmniSharp.Extensions.DebugAdapter.Protocol.Models;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using Microsoft.Extensions.Logging;
using System.Buffers;

using System.IO.Pipelines;
using System.Text;
using OmniSharp.Extensions.DebugAdapter.Server;
using OmniSharp.Extensions.JsonRpc;

using System.Runtime.InteropServices;

internal static class Program
{
    public static Task Main() => RunAsync();

    [UnmanagedCallersOnly(EntryPoint = "ProbeRun")]
    public static int RunNative()
    {
        try { RunAsync().GetAwaiter().GetResult(); return 0; }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }

    private static async Task RunAsync()
    {
var input = new Pipe();
var output = new Pipe();
Console.WriteLine("creating server");
using var server = DebugAdapterServer.Create(o => o.WithInput(input.Reader).WithOutput(output.Writer).WithLoggerFactory(new ProbeLoggerFactory()).WithUnhandledExceptionHandler(ex => Console.Error.WriteLine("UNHANDLED " + ex)).OnAttach((AttachRequestArguments request, CancellationToken token) => {
    if (!request.ExtensionData.TryGetValue("sessionToken", out var value) || value.ToString() != "安全") throw new InvalidOperationException("The session token was not preserved.");
    return Task.FromResult(new AttachResponse());
}).OnSetBreakpoints((SetBreakpointsArguments request, CancellationToken token) => {
    if (request.Source.Path != "/tmp/测试.cs" || request.Breakpoints?.Single().Line != 7) throw new InvalidOperationException("The breakpoint request was not preserved.");
    return Task.FromResult(new SetBreakpointsResponse { Breakpoints = new[] { new Breakpoint { Verified = true, Line = 7 } } });
}).OnVariables((VariablesArguments request, CancellationToken token) => {
    if (request.VariablesReference != 42 || request.Filter != VariablesArgumentsFilter.Named) throw new InvalidOperationException("Variables arguments were not preserved.");
    return Task.FromResult(new VariablesResponse { Variables = new[] { new Variable { Name = "answer", Value = "123", VariablesReference = 0 } } });
}).OnEvaluate((EvaluateArguments request, CancellationToken token) => {
    if (request.Expression != "answer + 1" || request.Context != EvaluateArgumentsContext.Watch) throw new InvalidOperationException("Evaluate arguments were not preserved.");
    return Task.FromResult(new EvaluateResponse { Result = "124", VariablesReference = 0 });
}).OnThreads((ThreadsArguments request, CancellationToken token) => Task.FromResult(new ThreadsResponse {
    Threads = new[] { new OmniSharp.Extensions.DebugAdapter.Protocol.Models.Thread { Id = 42, Name = "main" } }
})).OnStackTrace((StackTraceArguments request, CancellationToken token) => Task.FromResult(new StackTraceResponse {
    StackFrames = new[] { new StackFrame { Id = 7, Name = "Main", Line = 7, Column = 1, Source = new Source { Path = "/tmp/测试.cs" } } }
})).OnScopes((ScopesArguments request, CancellationToken token) => Task.FromResult(new ScopesResponse {
    Scopes = new[] { new Scope { Name = "Locals", VariablesReference = 42, Expensive = false } }
})).OnNext((NextArguments request, CancellationToken token) => Task.FromResult(new NextResponse()))
.OnJsonRequest("customProbe", (JsonElement request, CancellationToken token) => {
    if (request.GetProperty("message").GetString() != "中文") throw new InvalidOperationException("Custom request was not preserved.");
    using var document = JsonDocument.Parse("{\"result\":\"ok\"}");
    return Task.FromResult(document.RootElement.Clone());
}));
Console.WriteLine("server created");
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var initialization = server.Initialize(timeout.Token);
const string json = "{\"seq\":1,\"type\":\"request\",\"command\":\"initialize\",\"arguments\":{\"adapterID\":\"probe\",\"clientID\":\"probe\",\"linesStartAt1\":true,\"columnsStartAt1\":true}}";
byte[] body = Encoding.UTF8.GetBytes(json);
await input.Writer.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n"));
await input.Writer.WriteAsync(body);
string response = "";
while (!response.Contains("\"success\":true"))
{
    var result = await output.Reader.ReadAsync(timeout.Token);
    response += Encoding.UTF8.GetString(result.Buffer.ToArray());
    output.Reader.AdvanceTo(result.Buffer.End);
}
Console.WriteLine(response);
Console.WriteLine("initialize successful");
const string attach = "{\"seq\":2,\"type\":\"request\",\"command\":\"attach\",\"arguments\":{\"sessionToken\":\"安全\"}}";
body = Encoding.UTF8.GetBytes(attach);
await input.Writer.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n"));
await input.Writer.WriteAsync(body);
response = "";
while (!response.Contains("\"request_seq\":2"))
{
    var result = await output.Reader.ReadAsync(timeout.Token);
    response += Encoding.UTF8.GetString(result.Buffer.ToArray());
    output.Reader.AdvanceTo(result.Buffer.End);
}
Console.WriteLine(response);
if (!response.Contains("\"success\":true")) throw new Exception("Attach failed.");
Console.WriteLine("attach successful");
server.SendStopped(new StoppedEvent { Reason = StoppedEventReason.Breakpoint, ThreadId = 42, AllThreadsStopped = true });
response = "";
while (!response.Contains("\"event\":\"stopped\""))
{
    var result = await output.Reader.ReadAsync(timeout.Token);
    response += Encoding.UTF8.GetString(result.Buffer.ToArray());
    output.Reader.AdvanceTo(result.Buffer.End);
}
if (!response.Contains("\"reason\":\"breakpoint\"") || !response.Contains("\"threadId\":42")) throw new Exception("Stopped event was not preserved.");
Console.WriteLine("stopped event successful");
const string setBreakpoints = "{\"seq\":3,\"type\":\"request\",\"command\":\"setBreakpoints\",\"arguments\":{\"source\":{\"path\":\"/tmp/测试.cs\"},\"breakpoints\":[{\"line\":7}]}}";
body = Encoding.UTF8.GetBytes(setBreakpoints);
await input.Writer.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n"));
await input.Writer.WriteAsync(body);
response = "";
while (!response.Contains("\"request_seq\":3"))
{
    var result = await output.Reader.ReadAsync(timeout.Token);
    response += Encoding.UTF8.GetString(result.Buffer.ToArray());
    output.Reader.AdvanceTo(result.Buffer.End);
}
Console.WriteLine(response);
if (!response.Contains("\"success\":true") || !response.Contains("\"verified\":true")) throw new Exception("Setting breakpoints failed.");
Console.WriteLine("breakpoints successful");
await RequestAndCheckAsync("{\"seq\":4,\"type\":\"request\",\"command\":\"variables\",\"arguments\":{\"variablesReference\":42,\"filter\":\"Named\"}}", 4, "\"value\":\"123\"");
await RequestAndCheckAsync("{\"seq\":5,\"type\":\"request\",\"command\":\"evaluate\",\"arguments\":{\"expression\":\"answer + 1\",\"context\":\"watch\"}}", 5, "\"result\":\"124\"");
await RequestAndCheckAsync("{\"seq\":6,\"type\":\"request\",\"command\":\"setBreakpoints\",\"arguments\":{\"source\":{\"path\":42}}}", 6, "\"success\":false", false);
await RequestAndCheckAsync("{\"seq\":7,\"type\":\"request\",\"command\":\"threads\",\"arguments\":{}}", 7, "\"name\":\"main\"");
await RequestAndCheckAsync("{\"seq\":8,\"type\":\"request\",\"command\":\"stackTrace\",\"arguments\":{\"threadId\":42}}", 8, "\"name\":\"Main\"");
await RequestAndCheckAsync("{\"seq\":9,\"type\":\"request\",\"command\":\"scopes\",\"arguments\":{\"frameId\":7}}", 9, "\"variablesReference\":42");
await RequestAndCheckAsync("{\"seq\":10,\"type\":\"request\",\"command\":\"next\",\"arguments\":{\"threadId\":42,\"granularity\":\"statement\"}}", 10, "\"success\":true");
await RequestAndCheckAsync("{\"seq\":11,\"type\":\"request\",\"command\":\"customProbe\",\"arguments\":{\"message\":\"中文\"}}", 11, "\"result\":\"ok\"");
await input.Writer.CompleteAsync();

async Task RequestAndCheckAsync(string request, int id, string expected, bool success = true)
{
    byte[] data = Encoding.UTF8.GetBytes(request);
    await input.Writer.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {data.Length}\r\n\r\n"));
    await input.Writer.WriteAsync(data);
    string received = "";
    while (!received.Contains($"\"request_seq\":{id}"))
    {
        var result = await output.Reader.ReadAsync(timeout.Token);
        received += Encoding.UTF8.GetString(result.Buffer.ToArray());
        output.Reader.AdvanceTo(result.Buffer.End);
    }
    Console.WriteLine(received);
    if (!received.Contains(success ? "\"success\":true" : "\"success\":false") || !received.Contains(expected)) throw new Exception($"Request {id} failed.");
    Console.WriteLine($"request {id} successful");
}

    }
}

sealed class ProbeLoggerFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new ProbeLogger(categoryName);
    public void Dispose() { }
}
sealed class ProbeLogger(string category) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new ProbeScope();
    public bool IsEnabled(LogLevel level) => true;
    public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Console.Error.WriteLine($"{category} {level}: {formatter(state, exception)} {exception}");
}

sealed class ProbeScope : IDisposable { public void Dispose() { } }
