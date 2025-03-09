using System;

namespace API.Helpers;

public class MessageParams : PaginationParms
{
    public string? Username { get; set; }
    public required string Container { get; set; } = "Unread";
}   
