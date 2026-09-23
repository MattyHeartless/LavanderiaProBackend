using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Services;

public sealed class DeliveryModeConflictException(string message) : Exception(message);

public sealed class DeliveryModeCatalogService(OrdersDbContext context)
{
    private const int DefaultDeliveryModeId = 4;

    public Task<List<DeliveryMode>> ListAsync(CancellationToken cancellationToken = default) =>
        context.DeliveryModes.AsNoTracking()
            .OrderBy(mode => mode.SortOrder).ThenBy(mode => mode.Id)
            .ToListAsync(cancellationToken);

    public Task<DeliveryMode?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        context.DeliveryModes.AsNoTracking().FirstOrDefaultAsync(mode => mode.Id == id, cancellationToken);

    public async Task<DeliveryMode> CreateAsync(DeliveryModeSaveRequest request, CancellationToken cancellationToken = default)
    {
        var mode = BuildMode(request);
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        if (await CodeExistsAsync(mode.Code, null, cancellationToken))
            throw new DeliveryModeConflictException("Ya existe un modo de entrega con ese código.");

        mode.Id = await context.Database.SqlQueryRaw<int>(
            "SELECT ISNULL(MAX([Id]), 0) + 1 AS [Value] FROM [DeliveryModes] WITH (UPDLOCK, HOLDLOCK)")
            .SingleAsync(cancellationToken);
        context.DeliveryModes.Add(mode);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new DeliveryModeConflictException("No se pudo crear el modo: el código o identificador ya existe.");
        }
        return mode;
    }

    public async Task<DeliveryMode?> UpdateAsync(int id, DeliveryModeSaveRequest request, CancellationToken cancellationToken = default)
    {
        var changes = BuildMode(request);
        var mode = await context.DeliveryModes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (mode is null) return null;

        if (id == DefaultDeliveryModeId && !changes.IsActive)
            throw new DeliveryModeConflictException("El modo predeterminado debe permanecer activo.");
        if (await CodeExistsAsync(changes.Code, id, cancellationToken))
            throw new DeliveryModeConflictException("Ya existe un modo de entrega con ese código.");

        mode.Code = changes.Code;
        mode.Name = changes.Name;
        mode.EtaHours = changes.EtaHours;
        mode.SurchargeAmount = changes.SurchargeAmount;
        mode.IsActive = changes.IsActive;
        mode.SortOrder = changes.SortOrder;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new DeliveryModeConflictException("No se pudo actualizar el modo: el código ya existe.");
        }
        return mode;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var mode = await context.DeliveryModes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (mode is null) return false;
        if (id == DefaultDeliveryModeId)
            throw new DeliveryModeConflictException("El modo predeterminado no se puede eliminar.");
        if (await context.Orders.AnyAsync(order => order.DeliveryModeId == id, cancellationToken))
            throw new DeliveryModeConflictException("Este modo ya está asociado a pedidos. Desactívalo en lugar de eliminarlo.");

        context.DeliveryModes.Remove(mode);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new DeliveryModeConflictException("Este modo ya está asociado a pedidos. Desactívalo en lugar de eliminarlo.");
        }
        return true;
    }

    private Task<bool> CodeExistsAsync(string code, int? exceptId, CancellationToken cancellationToken) =>
        context.DeliveryModes.AnyAsync(mode => mode.Code == code && (!exceptId.HasValue || mode.Id != exceptId.Value), cancellationToken);

    private static DeliveryMode BuildMode(DeliveryModeSaveRequest request)
    {
        var code = request.Code?.Trim().ToUpperInvariant() ?? string.Empty;
        var name = request.Name?.Trim() ?? string.Empty;
        if (code.Length is < 1 or > 40 || !Regex.IsMatch(code, "^[A-Z0-9_]+$"))
            throw new ArgumentException("El código debe tener entre 1 y 40 caracteres y usar solo letras, números o guiones bajos.");
        if (name.Length is < 1 or > 100)
            throw new ArgumentException("El nombre debe tener entre 1 y 100 caracteres.");
        if (request.EtaHours is null or < 1 or > 720)
            throw new ArgumentException("El tiempo estimado debe estar entre 1 y 720 horas.");
        if (request.SurchargeAmount is null or < 0 or > 999999.99m || decimal.Round(request.SurchargeAmount.Value, 2) != request.SurchargeAmount.Value)
            throw new ArgumentException("El recargo debe estar entre $0 y $999,999.99, con máximo dos decimales.");
        if (request.SortOrder is null or < 1 or > 10000)
            throw new ArgumentException("El orden debe estar entre 1 y 10,000.");
        if (!request.IsActive.HasValue)
            throw new ArgumentException("Debes indicar si el modo está activo.");

        return new DeliveryMode
        {
            Code = code, Name = name, EtaHours = request.EtaHours.Value,
            SurchargeAmount = request.SurchargeAmount.Value,
            IsActive = request.IsActive.Value, SortOrder = request.SortOrder.Value
        };
    }
}
