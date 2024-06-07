namespace FekraHubAPI.ContractMaker
{
    public interface IContractMaker
    {
        Task ConverterHtmlToPdf(int studentId);
        Task<byte[]> GetContractPdf(int studentId);
        Task<List<string>> ContractHtml(int studentId);
    }
}
