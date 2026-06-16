namespace EcommerceCA.Common.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User  = "User";
}

public static class AppConstants
{
    public const decimal GstRate               = 0.18m;
    public const decimal FreeShippingThreshold = 500m;
    public const decimal StandardShippingCost  = 50m;
    public const int     DefaultPageSize       = 10;
    public const int     MaxPageSize           = 100;
}
