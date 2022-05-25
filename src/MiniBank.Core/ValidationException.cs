namespace MiniBank.Core;

public class ValidationException : System.Exception
{
    public ValidationException(string message) : base(message) {}
}