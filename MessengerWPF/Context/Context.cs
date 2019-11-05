using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerWPF
{
    public class Context : DbContext
    {
        public Context() : base("DbConnection") { Database.CreateIfNotExists(); }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }
        public DbSet<ChatHistory> ChatHistories { get; set; }
    }
}
