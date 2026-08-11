using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;

namespace MyWarehouse.Application.SystemSettings.GetFeedSettings;

public class GetFeedSettingsQuery : IRequest<FeedSettingsDto>
{
}

public class FeedSettingsDto
{
    public int MaxPostsToShow { get; init; } = 50;
}

public class GetFeedSettingsQueryHandler : IRequestHandler<GetFeedSettingsQuery, FeedSettingsDto>
{
    private const int DefaultMaxPostsToShow = 50;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetFeedSettingsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<FeedSettingsDto> Handle(GetFeedSettingsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

        var settings = await _unitOfWork.FeedSettings!.GetByUserIdAsync(currentUserId);

        return new FeedSettingsDto
        {
            MaxPostsToShow = settings?.MaxPostsToShow ?? DefaultMaxPostsToShow
        };
    }
}
