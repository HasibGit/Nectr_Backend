using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Data;

public class LikeRepository : ILikeRepository
{
    public void AddLike(Like like)
    {
        throw new NotImplementedException();
    }

    public void DeleteLike(Like like)
    {
        throw new NotImplementedException();
    }

    public Task<Like?> GetLike(int sourceUserId, int targetUserId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<int>> GetLikeIds(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<MemberDto>> GetLikes(string predicate, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveChanges()
    {
        throw new NotImplementedException();
    }
}
