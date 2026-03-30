using Application.Abstraction.CQRS;
using CSharpFunctionalExtensions;

#pragma warning disable VSTHRD200 

namespace Application.Abstraction;

public interface IMediator
{
    Task<Result> Send<TCommand>( TCommand command, CancellationToken cancellationToken )
        where TCommand : ICommand;
    Task<Result<TResult>> Send<TCommand, TResult>( TCommand command, CancellationToken cancellationToken )
    where TCommand : ICommand<TResult>;
    Task<Result<TResult>> Query<TQuery, TResult>( TQuery query, CancellationToken cancellationToken )
        where TQuery : IQuery<TResult>;
}