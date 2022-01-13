namespace RemoteMediatr.Core;

public class ClientRequestException : Exception
{
    public ProblemInfo ProblemInfo { get; set; }

    public ClientRequestException(ProblemInfo problemInfo)
    {
        ProblemInfo = problemInfo;
    }

    public ClientRequestException(string message, IEnumerable<string>? errors = null)
    {
        ProblemInfo = new ProblemInfo(message, errors ?? Enumerable.Empty<string>());
    }
}
