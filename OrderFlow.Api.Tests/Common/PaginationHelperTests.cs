using OrderFlow.Api.Common;
using OrderFlow.Api.Exceptions;

namespace OrderFlow.Api.Tests.Common
{
    public class PaginationHelperTests
    {
        [Theory]
        [InlineData(1, 10, 0)]
        [InlineData(2, 10, 10)]
        [InlineData(3, 20, 40)]
        [InlineData(5, 25, 100)]
        public void CalculateOffset_ValidParameters_ReturnsCorrectOffset(int page, int pageSize, int expectedOffset)
        {
            var actualOffset = PaginationHelper.CalculateOffset(page, pageSize);
            Assert.Equal(expectedOffset, actualOffset);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(-1, 10)]
        [InlineData(3, 0)]
        [InlineData(1, 101)]
        [InlineData(100000000, 100)]
        public void CalculateOffset_InvalidParameters_ThrowsValidationException(int page, int pageSize)
        {
            Assert.Throws<ValidationException>(() => PaginationHelper.CalculateOffset(page, pageSize));
        }

        [Theory]
        [InlineData(25, 10, 3)]
        [InlineData(20, 10, 2)]
        [InlineData(5, 10, 1)]
        [InlineData(0, 10, 0)]
        [InlineData(101, 20, 6)]
        [InlineData(100, 100, 1)]
        public void CalculateTotalPages_ValidParameters_ReturnsCorrectPageCount(int totalCount, int pageSize, int expectedPages)
        {
            var actualPages = PaginationHelper.CalculateTotalPages(totalCount, pageSize);
            Assert.Equal(expectedPages, actualPages);
        }

        [Theory]
        [InlineData(25, 0)]
        [InlineData(25, -1)]
        [InlineData(25, 101)]
        public void CalculateTotalPages_InvalidPageSize_ThrowsValidationException(int totalCount, int pageSize)
        {
            Assert.Throws<ValidationException>(() => PaginationHelper.CalculateTotalPages(totalCount, pageSize));
        }
    }
}
