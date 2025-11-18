using semester_project.Domain.Entities;

namespace semester_project.Application.Ports;

public interface IMediaRepository
{
    Task<long> AddAsync(Media media);
    
    Task<long?> GetCreatorIdAsync(long mediaId); 
    Task<bool>  DeleteAsync(long mediaId);
}