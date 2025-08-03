using System;


public class PaginationResponse<T>
{
    public int PageNumbre { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public ICollection<T>? Items { get; set; }= new List<T>();
}

