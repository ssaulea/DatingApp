using System;

namespace API.Helpers;

public class PaginationParms
{
    private const int MaxPageSize = 50;

    public int PageNumber { get; set; } = 1;
    private int pageSize = 10; 
    public int PageSize
    {
        get => pageSize;
        set => pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}
