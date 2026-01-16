namespace Gplant.API.ApiResponse
{
    public record SuccessResponse<T>
    (
        int StatusCode,
        string Message,
        T? Data,
        DateTime Timestamp
    );
}
