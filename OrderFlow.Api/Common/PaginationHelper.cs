using Azure;
using OrderFlow.Api.Exceptions;

namespace OrderFlow.Api.Common
{
    public static class PaginationHelper
    {
        public static int CalculateOffset(int page, int pageSize)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                throw new ValidationException("Invalid pagination parameters.");

            var offset = ((long)page - 1) * pageSize;

            if (offset > int.MaxValue)
                throw new ValidationException("Pagination offset is too large.");

            return (int)offset;
        }

        public static int CalculateTotalPages(int totalCount, int pageSize)
        {
            if (pageSize < 1 || pageSize > 100)
                throw new ValidationException("Invalid pagination parameters.");

            return (int)Math.Ceiling((double)totalCount / pageSize);
        }
    }
}
