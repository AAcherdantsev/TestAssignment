namespace TestAssignment.ApiService.ApiResponse;

public record GetResponse<T>(bool Success, T ResourceData, ErrorType? ErrorType);