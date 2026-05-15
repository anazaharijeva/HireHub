namespace AuthService.Domain;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Recruiter = "Recruiter";
    public const string Candidate = "Candidate";

    public static readonly IReadOnlyList<string> All = new[] { Admin, Recruiter, Candidate };
}
