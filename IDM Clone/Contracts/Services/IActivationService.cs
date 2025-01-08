namespace IDM_Clone.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
