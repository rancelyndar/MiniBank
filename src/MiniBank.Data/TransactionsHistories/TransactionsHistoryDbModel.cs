using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBank.Core.BankAccounts;

namespace MiniBank.Core.TransactionsHistories;

public class TransactionsHistoryDbModel
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public string FromAccountId { get; set; }
    public string ToAccountId { get; set; }
    public BankAccountDbModel FromAccount { get; set; }
    public BankAccountDbModel ToAccount { get; set; }
    
    internal class Map : IEntityTypeConfiguration<TransactionsHistoryDbModel>
    {
        public void Configure(EntityTypeBuilder<TransactionsHistoryDbModel> builder)
        {
            builder.ToTable("transaction");

            builder.HasKey(it => it.Id).HasName("pk_transactions_history");
            builder.HasOne(it => it.FromAccount)
                .WithMany(it => it.FromTransactions)
                .HasForeignKey(it => it.FromAccountId);
            builder.HasOne(it => it.ToAccount)
                .WithMany(it => it.ToTransactions)
                .HasForeignKey(it => it.ToAccountId);
        }
    }
}