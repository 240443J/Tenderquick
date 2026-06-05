using TenderquickServer.Models;
using TenderquickServer.Models.Tenders;

namespace TenderquickServer.Services
{
    public class DuplicateReferenceException : Exception
    {
        public DuplicateReferenceException(string reference)
            : base($"A tender with reference '{reference}' already exists.") { }
    }

    public interface ITenderService
    {
        Task<IEnumerable<TenderListItem>> GetAllAsync(string? status, string? search);
        Task<Tender?> GetByIdAsync(int id);
        Task<Tender> CreateAsync(CreateTenderRequest req);
        Task<Tender?> UpdateAsync(int id, UpdateTenderRequest req);
        Task<bool> DeleteAsync(int id);
    }
}
