using Xunit;
using LogisticsManagementSystem.Services;

namespace LogisticsManagementSystem.Tests
{
    public class FileValidationServiceTests
    {
        private readonly FileValidationService _service = new();

        [Fact]
        public void ValidatePdf_ThrowsException_ForInvalidFile()
        {
            var exception = Assert.Throws<Exception>(() =>
                _service.ValidatePdf("document.exe"));

            Assert.Equal("Only PDF files are allowed", exception.Message);
        }

        [Fact]
        public void ValidatePdf_DoesNotThrow_ForValidPdf()
        {
            // These should NOT throw exceptions
            _service.ValidatePdf("contract.pdf");
            _service.ValidatePdf("agreement.PDF");
        }
    }
}