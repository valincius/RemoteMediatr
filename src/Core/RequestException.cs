namespace RemoteMediatr.Core;

public class RequestException : Exception
{
    public ProblemInfo ProblemInfo { get; set; }

    public RequestException(ProblemInfo problemInfo)
    {
        ProblemInfo = problemInfo;
    }

    public RequestException(string message)
    {
        ProblemInfo = new ProblemInfo(message);
    }
}
