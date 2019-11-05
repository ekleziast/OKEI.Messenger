using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerWPF
{
    [Table("ChatMembers")]
    public class ChatMember
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Chat")]
        public Guid ChatID { get; set; }
        public virtual Chat Chat { get; set; }

        [ForeignKey("Account")]
        public Guid AccountID { get; set; }
        public Account Account { get; set; }
    }
}
