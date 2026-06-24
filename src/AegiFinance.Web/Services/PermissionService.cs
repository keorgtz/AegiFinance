namespace AegiFinance.Web.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string permissionCode);
    Task<bool> IsAdminAsync();
}
