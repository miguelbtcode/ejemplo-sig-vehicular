using Identity.Roles.Dtos;

namespace Identity.Roles.Features.CreateRole;

public record CreateRoleCommand(CreateRoleDto Role) : ICommand<CreateRoleResult>;

public record CreateRoleResult(Guid Id);

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Role.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Role.Description).MaximumLength(255);
    }
}

internal class CreateRoleHandler(IdentityDbContext dbContext)
    : ICommandHandler<CreateRoleCommand, CreateRoleResult>
{
    public async Task<CreateRoleResult> HandleAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        var role = Role.Create(command.Role.Name, command.Role.Description);

        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateRoleResult(role.Id);
    }
}
