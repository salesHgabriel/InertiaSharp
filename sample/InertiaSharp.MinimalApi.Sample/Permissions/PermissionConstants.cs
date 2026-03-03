namespace InertiaSharp.MinimalApi.Sample.Permissions;

public static class Roles
{
    public const string Admin  = "Admin";
    public const string Editor = "Editor";
    public const string Viewer = "Viewer";
}

public static class Policies
{
    public const string CanEditContent = "CanEditContent";
    public const string CanManageUsers = "CanManageUsers";
    public const string CanViewReports = "CanViewReports";
}
