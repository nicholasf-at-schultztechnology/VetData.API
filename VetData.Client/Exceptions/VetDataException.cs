namespace VetData.Client.Exceptions;

public class VetDataException : Exception
{
    public VetDataException(string message) : base(message) { }
    public VetDataException(string message, Exception innerException) 
        : base(message, innerException) { }
}
