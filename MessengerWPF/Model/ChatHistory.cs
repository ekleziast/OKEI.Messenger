using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerWPF
{
    [Table("ChatHistory")]
    public class ChatHistory
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Chat")]
        public Guid ChatID { get; set; }
        public virtual Chat Chat { get; set; }

        public DateTime DateTime { get; set; }
        public byte[] Message { get; set; }
        public bool IsFile { get; set; }
    }
}
