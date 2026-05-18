namespace LogisticsManagementSystem.Services
{
    public class FileValidationService
    {
        public void ValidatePdf(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            if (extension != ".pdf")
            {
                throw new Exception("Only PDF files are allowed");
            }
        }
    }
}
