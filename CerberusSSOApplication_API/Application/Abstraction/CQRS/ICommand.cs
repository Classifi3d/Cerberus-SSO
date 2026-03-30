namespace Application.Abstraction.CQRS;

public interface ICommand : IBaseCommand { }

public interface ICommand<TReponse> : IBaseCommand { }

public interface IBaseCommand { }
