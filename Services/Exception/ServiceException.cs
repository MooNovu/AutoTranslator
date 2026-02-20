using System;

namespace AutoTranslator.Services.Exception;

public class ServiceException(string message, bool canRetry = false, System.Exception? inner = null) : 
    System.Exception(message, inner)
{
    public bool CanRetry { get; } = canRetry;
}
public class LlmException(string message, bool canRetry = true, System.Exception? inner = null) : 
    ServiceException(message, canRetry, inner)
{
}

public class OcrException(string message, bool canRetry = false, System.Exception? inner = null) : 
    ServiceException(message, canRetry, inner)
{
}