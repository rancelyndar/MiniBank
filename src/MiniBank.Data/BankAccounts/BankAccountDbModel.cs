using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBank.Core.TransactionsHistories;
using MiniBank.Core.Users;

namespace MiniBank.Core.BankAccounts;

public class BankAccountDbModel
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public bool IsOpen { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public UserDbModel User { get; set; }
    public List<TransactionsHistoryDbModel> ToTransactions { get; set; }
    public List<TransactionsHistoryDbModel> FromTransactions { get; set; }
    
    internal class Map : IEntityTypeConfiguration<BankAccountDbModel>
    {
        public void Configure(EntityTypeBuilder<BankAccountDbModel> builder)
        {
            builder.ToTable("bank_account");
            
            builder.Property(it => it.Currency)
                .HasConversion(
                    it => it.ToString(),
                    it => (Currency)Enum.Parse(typeof(Currency), it));;

            builder.HasKey(it => it.Id).HasName("pk_bank_accounts");
            builder.HasOne(it => it.User)
                .WithMany(it => it.Accounts)
                .HasForeignKey(it => it.UserId);
        }
    }
}