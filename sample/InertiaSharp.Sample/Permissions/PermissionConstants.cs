namespace InertiaSharp.Sample.Permissions;

/// <summary>
/// Role name constants. Add these roles to Identity via seeding.
/// </summary>
public static class Roles
{
    public const string Admin  = "Admin";
    public const string Editor = "Editor";
    public const string Viewer = "Viewer";
}

/// <summary>
/// Policy name constants registered in Program.cs.
/// Use [Authorize(Policy = Policies.CanEditContent)] on controllers/actions.
/// </summary>
public static class Policies
{
    public const string CanEditContent  = "CanEditContent";
    public const string CanManageUsers  = "CanManageUsers";
    public const string CanViewReports  = "CanViewReports";
}
