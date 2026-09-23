using FluentResults;
using Mediator;

namespace Bank.Clients.Commands.DeleteClient;

public sealed record DeleteClientCommand(int Id) : ICommand<Result>;
