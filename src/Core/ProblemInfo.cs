namespace RemoteMediatr.Core;
public record ProblemInfo(
    string Title,
    IEnumerable<string>? Errors = null
);