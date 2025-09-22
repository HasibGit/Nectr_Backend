using API.DTOs;
using API.Entities;
using API.Helper;
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

    public async Task<PagedList<MemberDto>> GetLikes(LikesParams likesParams)
    {
        var likes = context.Likes.AsQueryable();
        IQueryable<MemberDto> query;

        switch (likesParams.Predicate)
        {
            case "liked":
                query = likes.Where(x => x.SourceUserId == likesParams.UserId)
                                  .Select(x => x.TargetUser)
                                  .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;

            case "likedBy":
                query = likes.Where(x => x.TargetUserId == likesParams.UserId)
                                 .Select(x => x.SourceUser)
                                 .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
            default:
                var likedIds = await GetLikeIds(likesParams.UserId);

                query = likes.Where(x => x.TargetUserId == likesParams.UserId && likedIds.Contains(x.SourceUserId))
                                  .Select(x => x.SourceUser)
                                  .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
        }

        return await PagedList<MemberDto>.CreateAsync(
            query,
            likesParams.PageNumber,
            likesParams.PageSize
        );
    }
}
