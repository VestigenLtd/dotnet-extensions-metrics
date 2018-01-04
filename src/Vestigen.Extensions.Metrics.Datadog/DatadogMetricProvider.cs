using System;
using System.Collections.Concurrent;
using Vestigen.Extensions.Metrics.Abstractions;

namespace Vestigen.Extensions.Metrics.Datadog
{
    public class DatadogMetricProvider : IMetricProvider
    {
        private readonly ConcurrentDictionary<string, DatadogMetric> _metrics = new ConcurrentDictionary<string, DatadogMetric>();

        private IDatadogMetricSettings _settings;

        public DatadogMetricProvider(IDatadogMetricSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _settings.ChangeToken?.RegisterChangeCallback(OnConfigurationReload, null);
        }

        private void OnConfigurationReload(object state)
        {
            // The settings object needs to change as the old one is probably holding an old change token.
            _settings = _settings.Reload();

            // The token will change each time it reloads, so we need to register again.
            _settings?.ChangeToken?.RegisterChangeCallback(OnConfigurationReload, null);
        }

        public IMetric CreateMetric(string name)
        {
            return _metrics.GetOrAdd(name, CreateMetricImplementation);
        }

        private DatadogMetric CreateMetricImplementation(string name)
        {
            return new DatadogMetric(name);
        }

        public void Dispose()
        {
        }
    }
}