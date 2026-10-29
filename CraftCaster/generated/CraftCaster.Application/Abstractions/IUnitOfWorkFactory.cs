namespace CraftCaster.Application.Abstractions;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}