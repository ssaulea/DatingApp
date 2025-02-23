using System;

namespace API.Helpers;

public class LikesParams : PaginationParms
{
    public int UserId { get; set; }
    public required string Predicate { get; set; } = "liked";
}
