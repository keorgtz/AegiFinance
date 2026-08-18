using System.Data.Common;
using AegiFinance.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AegiFinance.Infrastructure.Data.Interceptors;

public sealed class TenantSessionContextInterceptor : DbConnectionInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public TenantSessionContextInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetSessionContext(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await SetSessionContextAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private void SetSessionContext(DbConnection connection)
    {
        using var command = CreateCommand(connection);
        command.ExecuteNonQuery();
    }

    private async Task SetSessionContextAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private DbCommand CreateCommand(DbConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "EXEC sys.sp_set_session_context @key=N'AegiFinance.IsClient', @value=@isClient; EXEC sys.sp_set_session_context @key=N'AegiFinance.ClientId', @value=@clientId; EXEC sys.sp_set_session_context @key=N'AegiFinance.OrganizationId', @value=@organizationId;";
        var isClient = command.CreateParameter();
        isClient.ParameterName = "@isClient";
        isClient.Value = string.Equals(_currentUser.UserType, "Client", StringComparison.OrdinalIgnoreCase);
        command.Parameters.Add(isClient);
        var clientId = command.CreateParameter();
        clientId.ParameterName = "@clientId";
        clientId.Value = _currentUser.ClientId?.ToString() ?? (object)DBNull.Value;
        command.Parameters.Add(clientId);
        var organizationId = command.CreateParameter();
        organizationId.ParameterName = "@organizationId";
        organizationId.Value = _currentUser.OrganizationId?.ToString() ?? (object)DBNull.Value;
        command.Parameters.Add(organizationId);
        return command;
    }
}
