using System;
using System.Collections.Generic;

namespace EcommerceCA.Common.Exceptions;

public class StockValidationException : Exception
{
    public List<StockErrorDto> StockErrors { get; }

    public StockValidationException(List<StockErrorDto> stockErrors, string message = "Some items have stock issues") 
        : base(message)
    {
        StockErrors = stockErrors;
    }
}

public class StockErrorDto
{
    public string Name { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public int Available { get; set; }
    public int? Requested { get; set; }
}
