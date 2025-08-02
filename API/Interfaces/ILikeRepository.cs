using API.DTOs;
using API.Entities;
using API.Helper;

namespace API.Interfaces;

public interface ILikeRepository
{
    Task<Like?> GetLike(int sourceUserId, int targetUserId);
    Task<PagedList<MemberDto>> GetLikes(LikesParams likesParams);
    Task<IEnumerable<int>> GetLikeIds(int userId);
    void DeleteLike(Like like);
    void AddLike(Like like);
    Task<bool> SaveChanges();
}
