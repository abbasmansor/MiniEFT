namespace MiniEft.Application.Exceptions;

///  HTTP 400.
public sealed class ValidationException(string message) : Exception(message);

///  HTTP 404.
public sealed class NotFoundException(string message) : Exception(message);

///  HTTP 409.
public sealed class ConflictException(string message) : Exception(message);

public sealed class DuplicateKeyException(string message) : Exception(message);
