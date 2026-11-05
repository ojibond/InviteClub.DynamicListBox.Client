namespace SportsClub.Server;

/// <summary>Thrown on a business-rule conflict (duplicate name, concurrent edit). Maps to HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
