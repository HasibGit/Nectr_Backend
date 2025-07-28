using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikeRepository(DataContext context, IMapper mapper) : ILikeRepository
{
    public void AddLike(Like like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(Like like)
    {
        context.Likes.Remove(like);
    }

    public async Task<Like?> GetLike(int sourceUserId, int targetUserId)
    {
        return await context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<IEnumerable<int>> GetLikeIds(int userId)
    {
        return await context.Likes
                            .Where(x => x.SourceUserId == userId)
                            .Select(x => x.TargetUserId)
                            .ToListAsync();
    }

    public async Task<IEnumerable<MemberDto>> GetLikes(string predicate, int userId)
    {
        var likes = context.Likes.AsQueryable();

        switch (predicate)
        {
            case "liked":
                return await likes.Where(x => x.SourceUserId == userId)
                                  .Select(x => x.TargetUser)
                                  .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                                  .ToListAsync();

            case "likedBy":
                return await likes.Where(x => x.TargetUserId == userId)
                                 .Select(x => x.SourceUser)
                                 .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                                 .ToListAsync();
            default:
                var likedIds = await GetLikeIds(userId);

                return await likes.Where(x => x.TargetUserId == userId && likedIds.Contains(x.SourceUserId))
                                  .Select(x => x.SourceUser)
                                  .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                                  .ToListAsync();
        }
    }

    public async Task<bool> SaveChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
