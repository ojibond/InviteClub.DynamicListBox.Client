namespace SportsClub.Server;

/// <summary>Thrown when a requested resource does not exist. Maps to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Thrown on a business-rule conflict (duplicate name, concurrent edit). Maps to HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
