namespace LibraryApp.Application.Exceptions
{
    public class PublisherNotFoundException : Exception
    {
        public int PublisherId { get; }

        public PublisherNotFoundException(int publisherId) 
            : base($"Publisher with ID {publisherId} was not found.")
        {
            PublisherId = publisherId;
        }

        public PublisherNotFoundException(int publisherId, string message) 
            : base(message)
        {
            PublisherId = publisherId;
        }

        public PublisherNotFoundException(int publisherId, string message, Exception innerException) 
            : base(message, innerException)
        {
            PublisherId = publisherId;
        }
    }
} 