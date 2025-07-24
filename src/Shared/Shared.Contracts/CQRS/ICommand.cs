namespace Shared.Contracts.CQRS;

public interface ICommand<TResponse> { }

public interface ICommand : ICommand<Unit> { }
