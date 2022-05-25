using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBank.Core.BankAccounts;

namespace MiniBank.Core.Users;

public class UserDbModel
{
    public string Id { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public List<BankAccountDbModel> Accounts { get; set; }

    internal class Map : IEntityTypeConfiguration<UserDbModel>
    {
        public void Configure(EntityTypeBuilder<UserDbModel> builder)
        {
            builder.ToTable("user");

            builder.HasKey(it => it.Id);
        }
    }
}