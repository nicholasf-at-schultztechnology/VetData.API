namespace VetData.Client.Exceptions;

public class VetDataAuthenticationException : VetDataException
{
    public VetDataAuthenticationException(string message) : base(message) { }
    public VetDataAuthenticationException(string message, Exception inner) : base(message, inner) { }
}