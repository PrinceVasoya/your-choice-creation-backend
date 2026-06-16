namespace EcommerceCA.Common.Responses;

public class ApiResponse<T>
{
    public bool                  Success    { get; set; }
    public string                Message    { get; set; } = string.Empty;
    public T?                    Data       { get; set; }
    public IEnumerable<string>?  Errors     { get; set; }
    public int                   StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") => new()
    {
        Success = true, Message = message, Data = data, StatusCode = 200
    };

    public static ApiResponse<T> Created(T data, string message = "Created successfully.") => new()
    {
        Success = true, Message = message, Data = data, StatusCode = 201
    };

    public static ApiResponse<T> Fail(string message, int statusCode = 400,
        IEnumerable<string>? errors = null) => new()
    {
        Success = false, Message = message, Errors = errors, StatusCode = statusCode
    };
}

public class PaginatedResponse<T>
{
    public bool             Success { get; set; } = true;
    public string           Message { get; set; } = "Success";
    public List<T>          Data    { get; set; } = new();
    public PaginationMeta   Meta    { get; set; } = new();
}

public class PaginationMeta
{
    public int  TotalCount  { get; set; }
    public int  Page        { get; set; }
    public int  PageSize    { get; set; }
    public int  TotalPages  { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext     { get; set; }
}
