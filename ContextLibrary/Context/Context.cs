using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContextLibrary
{
    public class Context : DbContext
    {
        public Context() : base("DbConnection") { Database.CreateIfNotExists(); }
        /// <summary>
        /// Таблица с аккаунтами пользователей
        /// </summary>
        public DbSet<Person> Persons { get; set; }
        /// <summary>
        /// Таблица статусов:
        /// В сети, Рад поболтать, Занят, Отошел, Не в сети
        /// </summary>
        public DbSet<Status> Statuses { get; set; }
        /// <summary>
        /// Таблица с аватарками пользователей
        /// </summary>
        public DbSet<Photo> Photos { get; set; }
        /// <summary>
        /// Таблица с диалогами
        /// </summary>
        public DbSet<Conversation> Conversations { get; set; }
        /// <summary>
        /// Таблица с участниками диалогов
        /// </summary>
        public DbSet<Member> Members { get; set; }

    }
}
