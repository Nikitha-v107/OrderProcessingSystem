namespace Order.Domain.Exceptions
{
    public class InfrastructureException : Exception
    {
        public InfrastructureException(string message, Exception innerException) : base(message, innerException) { }
    }
}
