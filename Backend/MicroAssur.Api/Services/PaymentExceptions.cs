namespace MicroAssur.Api.Services;

public sealed class ResourceNotFoundException(string message) : Exception(message);

public sealed class RequestConflictException(string message) : Exception(message);
