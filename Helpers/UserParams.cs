using System;
using System.ComponentModel.DataAnnotations;

namespace API.Helpers;

public class UserParams : PaginationParms
{
   
    public string? Gender { get; set; }
    public string? CurrentUserName { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;

    public string OrderBy { get; set; } = "lastActive";
    
}
