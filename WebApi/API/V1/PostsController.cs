using MyWarehouse.Application.Posts.AcknowledgeComment;
using MyWarehouse.Application.Posts.AcknowledgePost;
using MyWarehouse.Application.Posts.AddComment;
using MyWarehouse.Application.Posts.CreatePost;
using MyWarehouse.Application.Posts.DeletePost;
using MyWarehouse.Application.Posts.GetPostsList;
using MyWarehouse.Application.UserActivity.Attributes;

namespace MyWarehouse.Infrastructure.API.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/posts")]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [LogList("Posts")]
    public async Task<ActionResult<List<PostDto>>> GetList([FromQuery] int? limit = null)
        => Ok(await _mediator.Send(limit.HasValue ? new GetPostsListQuery { Limit = limit.Value } : new GetPostsListQuery()));

    [HttpPost]
    [LogCreate("Post")]
    public async Task<ActionResult<int>> Create(CreatePostCommand command)
        => Ok(await _mediator.Send(command));

    [HttpDelete("{id}")]
    [LogDelete("Post")]
    public async Task<ActionResult> Delete(int id)
    {
        await _mediator.Send(new DeletePostCommand { Id = id });

        return NoContent();
    }

    [HttpPost("{id}/comments")]
    [LogCreate("PostComment")]
    public async Task<ActionResult<int>> AddComment(int id, AddCommentCommand command)
    {
        if (id != command.PostId) return BadRequest();

        return Ok(await _mediator.Send(command));
    }

    [HttpPost("{id}/acknowledge")]
    [LogUpdate("Post")]
    public async Task<ActionResult> AcknowledgePost(int id, AcknowledgePostCommand command)
    {
        if (id != command.PostId) return BadRequest();

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpPost("{id}/comments/{commentId}/acknowledge")]
    [LogUpdate("PostComment")]
    public async Task<ActionResult> AcknowledgeComment(int id, int commentId, AcknowledgeCommentCommand command)
    {
        if (id != command.PostId || commentId != command.CommentId) return BadRequest();

        await _mediator.Send(command);

        return NoContent();
    }
}
