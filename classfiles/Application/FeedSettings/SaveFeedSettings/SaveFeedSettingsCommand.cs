using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;

namespace MyWarehouse.Application.SystemSettings.SaveFeedSettings;

public class SaveFeedSettingsCommand : IRequest<Unit>
{
    public int MaxPostsToShow { get; init; }
}

public class SaveFeedSettingsCommandHandler : IRequestHandler<SaveFeedSettingsCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SaveFeedSettingsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(SaveFeedSettingsCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

        await _unitOfWork.FeedSettings!.UpsertAsync(currentUserId, request.MaxPostsToShow);

        return Unit.Value;
    }
}
