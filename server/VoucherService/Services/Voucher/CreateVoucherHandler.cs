using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Core.EF.Application;
using VoucherService.Persistence;
using VoucherService.DTOs;

namespace VoucherService.Services.Voucher;

public class CreateVoucherHandler : IRequestHandler<CreateVoucherCommand, AppResult<VoucherDto>>
{
    private readonly IVoucherUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateVoucherHandler(IVoucherUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppResult<VoucherDto>> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (!Enum.TryParse<Entities.Voucher.DiscountScope>(dto.Scope, ignoreCase: true, out var scope))
            return AppResult<VoucherDto>.Failure($"Invalid Scope '{dto.Scope}'", 400);

        if (!Enum.TryParse<Entities.Voucher.DiscountType>(dto.Type, ignoreCase: true, out var type))
            return AppResult<VoucherDto>.Failure($"Invalid Type '{dto.Type}'", 400);

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        var existing = await _unitOfWork.VoucherRepository.GetByCodeAsync(normalizedCode, cancellationToken);
        if (existing != null)
            return AppResult<VoucherDto>.Failure($"Voucher code '{normalizedCode}' already exists", 400);

        var voucher = new Entities.Voucher
        {
            Code = normalizedCode,
            Name = dto.Name,
            Description = dto.Description,
            Scope = scope,
            Type = type,
            Value = dto.Value,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MinOrderAmount = dto.MinOrderAmount,
            MaxUsageCount = dto.MaxUsageCount,
            MaxUsagePerUser = dto.MaxUsagePerUser,
            StartAt = dto.StartAt,
            ExpiredAt = dto.ExpiredAt,
            IsActive = dto.IsActive,
            ApplicableProductIds = dto.ApplicableProductIds
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _unitOfWork.VoucherRepository.AddAsync(voucher, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex) when (IsUniqueCodeViolation(ex))
        {
            // Concurrent create with the same Code raced past the existence check above
            // (TOCTOU) — the unique index caught it at insert time instead.
            // CommitTransactionAsync already rolled back the transaction in its own catch block.
            return AppResult<VoucherDto>.Failure("Mã voucher đã tồn tại", 409);
        }

        return AppResult<VoucherDto>.Success(_mapper.Map<VoucherDto>(voucher));
    }

    private static bool IsUniqueCodeViolation(Exception ex)
    {
        return ex is DbUpdateException { InnerException: PostgresException { SqlState: "23505" } };
    }
}
