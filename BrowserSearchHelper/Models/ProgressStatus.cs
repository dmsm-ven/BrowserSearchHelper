public class ProgressStatus
{
    public int Total { get; }
    public int Current { get; }

    public double Percent => (double)Current / Total;
    public int Left => Total - Current;

    public string StatusMessage => $"Выполнено: {Current} / {Total} ({Percent:P0})";

    public ProgressStatus(int current, int total)
    {
        Total = total;
        Current = current;
    }
}