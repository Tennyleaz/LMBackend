using Microsoft.AspNetCore.Http.HttpResults;

namespace LMBackend.Models;

public enum StreamStatus
{
    Created,
    InProgress,
    Completed,
    Failed
}
