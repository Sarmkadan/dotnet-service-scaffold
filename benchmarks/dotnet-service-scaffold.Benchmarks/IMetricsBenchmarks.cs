public interface IMetricsBenchmarks
{
    public void Setup();
    public void IncrementCounterNoTags();
    public void IncrementCounterOneTag();
    public void IncrementCounterThreeTags();
    public void RecordTimingNoTags();
    public void RecordTimingThreeTags();
    public void RecordGauge();
    public Task<Dictionary<string, object>> GetMetrics();
}