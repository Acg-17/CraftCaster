using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace CraftCaster.Infrastructure.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    private readonly object _gate = new();
    private readonly FileLoggerOptions _options;

    public FileLoggerProvider(FileLoggerOptions options)
    {
        _options = options;
        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public ILogger CreateLogger(string categoryName) =>
        new FileLogger(categoryName, this);

    public void Dispose()
    {
    }

    private void Write(LogEntry entry)
    {
        try
        {
            var json = JsonSerializer.Serialize(entry, SerializerOptions);
            lock (_gate)
            {
                AppendJsonArrayEntry(json);
            }
        }
        catch
        {
            // Logging must never break the database operation being logged.
        }
    }

    private void AppendJsonArrayEntry(string json)
    {
        var currentContent = File.Exists(_options.FilePath)
            ? File.ReadAllText(_options.FilePath)
            : string.Empty;
        var trimmedContent = currentContent.Trim();

        if (string.IsNullOrEmpty(trimmedContent) || !trimmedContent.StartsWith("[", StringComparison.Ordinal) || !trimmedContent.EndsWith("]", StringComparison.Ordinal))
        {
            File.WriteAllText(_options.FilePath, "[" + Environment.NewLine + json + Environment.NewLine + "]" + Environment.NewLine);
            return;
        }

        var entriesContent = trimmedContent[1..^1].Trim();
        var nextContent = string.IsNullOrEmpty(entriesContent)
            ? "[" + Environment.NewLine + json + Environment.NewLine + "]" + Environment.NewLine
            : "[" + Environment.NewLine + entriesContent + "," + Environment.NewLine + json + Environment.NewLine + "]" + Environment.NewLine;

        File.WriteAllText(_options.FilePath, nextContent);
    }

    private sealed class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly FileLoggerProvider _provider;

        public FileLogger(string categoryName, FileLoggerProvider provider)
        {
            _categoryName = categoryName;
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
            NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel != LogLevel.None && logLevel >= _provider._options.MinimumLevel;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var properties = ExtractProperties(state);
            var entry = new LogEntry
            {
                TimestampUtc = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd, HH:mm:ss", CultureInfo.InvariantCulture),
                Level = logLevel.ToString(),
                Category = _categoryName,
                EventId = eventId.Id,
                EventName = eventId.Name,
                Message = formatter(state, exception),
                BusinessOperation = GetStringProperty(properties, "BusinessOperation"),
                BusinessOperations = GetStringListProperty(properties, "BusinessOperations"),
                Operation = GetStringProperty(properties, "Operation"),
                Entity = GetStringProperty(properties, "Entity"),
                Properties = properties.Count == 0 ? null : properties,
                Exception = exception is null
                    ? null
                    : new LogException(exception.GetType().FullName, exception.Message, exception.StackTrace)
            };

            _provider.Write(entry);
        }

        private static Dictionary<string, object?> ExtractProperties<TState>(TState state)
        {
            var properties = new Dictionary<string, object?>();

            if (state is not IEnumerable<KeyValuePair<string, object?>> values)
            {
                return properties;
            }

            foreach (var value in values)
            {
                if (value.Key == "{OriginalFormat}")
                {
                    continue;
                }

                properties[value.Key] = value.Value;
            }

            return properties;
        }

        private static string? GetStringProperty(Dictionary<string, object?> properties, string key)
        {
            if (!properties.TryGetValue(key, out var value) || value is null)
            {
                return null;
            }

            return value.ToString();
        }

        private static IReadOnlyList<string>? GetStringListProperty(Dictionary<string, object?> properties, string key)
        {
            if (!properties.TryGetValue(key, out var value) || value is null)
            {
                return null;
            }

            if (value is IEnumerable<string> strings)
            {
                return strings.ToArray();
            }

            return null;
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }

    private sealed class LogEntry
    {
        public string TimestampUtc { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public int EventId { get; set; }

        public string? EventName { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? BusinessOperation { get; set; }

        public IReadOnlyList<string>? BusinessOperations { get; set; }

        public string? Operation { get; set; }

        public string? Entity { get; set; }

        public Dictionary<string, object?>? Properties { get; set; }

        public LogException? Exception { get; set; }
    }

    private sealed record LogException(string? Type, string Message, string? StackTrace);
}