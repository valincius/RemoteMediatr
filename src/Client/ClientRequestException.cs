using RemoteMediatr.Core;

namespace RemoteMediatr.Client;
public class ClientRequestException : Exception
{
    public ProblemInfo Problem { get; set;}

    public ClientRequestException(ProblemInfo problem)
    {
        Problem = problem;
    }
}
