using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using MiniBank.Core.BankAccounts.Services;
using MiniBank.Core.CurrencyConverterServices;
using MiniBank.Core.TransactionsHistories.Services;
using MiniBank.Core.Users.Services;

namespace MiniBank.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<ITransactionsHistoryService, TransactionsHistoryService>();
        services.AddFluentValidation().AddValidatorsFromAssembly(typeof(UserService).Assembly);
        return services;
    }
}