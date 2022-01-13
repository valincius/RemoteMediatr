namespace RemoteMediatr.Core;
public record ProblemInfo(string Title, IEnumerable<string> Errors);
public record ProblemInfoResponse(ProblemInfo ProblemInfo);