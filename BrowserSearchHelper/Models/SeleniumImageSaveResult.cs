public struct SeleniumImageSaveResult
{
    public bool IsSaved { get; private set; }
    public string Message { get; private set; }

    public SeleniumImageSaveResult(bool isSaved, string message)
    {
        IsSaved = isSaved;
        Message = message;
    }
}