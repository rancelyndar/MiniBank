using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniBank.Core.BankAccounts.Repositories;
using MiniBank.Core.CurrencyConverterServices;
using MiniBank.Core.TransactionsHistories.Repositories;
using MiniBank.Core.Users.Repositories;
using Microsoft.Extensions.Configuration;
using MiniBank.Data.CurrencyCourseServices;

namespace MiniBank.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ICurrencyCourseService, CurrencyCourseService>();
        services.AddHttpClient<ICurrencyCourseService, CurrencyCourseService>(options  =>
        {
            options.BaseAddress = new Uri(configuration["CurrencyConverterUri"]);
        });
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ITransactionsHistoryRepository, TransactionsHistoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddDbContext<MiniBankContext>(options => options.UseNpgsql(
            configuration["ConnectionStrings:DbConnectionString"]));
        return services;
    }
}
