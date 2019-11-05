using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerWPF
{
    [Table("Chat")]
    public class Chat
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ChatMember> ChatMembers { get; set; }
        public Chat()
        {
            ChatMembers = new List<ChatMember>();
        }
    }
}
