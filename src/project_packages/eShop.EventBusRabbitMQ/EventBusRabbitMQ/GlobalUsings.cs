global using System;
global using System.Collections.Generic;       // ← For Dictionary, IEnumerable
global using System.Diagnostics;               // ← For Activity, ActivitySource
global using System.Diagnostics.CodeAnalysis;  // ← For UnconditionalSuppressMessage
global using System.Linq;                      // ← For ServiceCollection extensions
global using System.Net;                       // ← For IPAddress, EndPoint
global using System.Net.Sockets;               // ← For SocketException
global using System.Text;                      // ← For Encoding
global using System.Text.Json;                 // ← For JsonSerializer
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Hosting;

global using RabbitMQ.Client;
global using RabbitMQ.Client.Events;
global using RabbitMQ.Client.Exceptions;

global using Polly;
global using Polly.Retry;
global using Polly.CircuitBreaker;

global using OpenTelemetry;
global using OpenTelemetry.Context.Propagation;
global using OpenTelemetry.Trace;

global using eShop.EventBus.Abstractions;
global using eShop.EventBus.Events;
global using eShop.EventBus.Extensions;