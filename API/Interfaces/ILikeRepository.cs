using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface ILikeRepository
{
    Task<Like?> GetLike(int sourceUserId, int targetUserId);
    Task<IEnumerable<MemberDto>> GetLikes(string predicate, int userId);
    Task<IEnumerable<int>> GetLikeIds(int userId);
    void DeleteLike(Like like);
    void AddLike(Like like);
    Task<bool> SaveChanges();
}
