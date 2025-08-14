namespace PhrazorApp.Commons.Results
{
    public static class ServiceResultNoContent
    {
        public static NoContentResult Success(string? message = null)
            => ServiceResult.Success(NoContent.Value, message);

        public static NoContentResult Warning(string? message = null)
            => ServiceResult.Warning(NoContent.Value, message);

        public static NoContentResult Error(string message)
            => ServiceResult.Error<NoContent>(message);
    }

}
