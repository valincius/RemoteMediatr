namespace RemoteMediatr.Core;
public record ProblemInfo(string Type, string Title, IEnumerable<string> Errors);
public record ProblemInfoResponse(ProblemInfo ProblemInfo);