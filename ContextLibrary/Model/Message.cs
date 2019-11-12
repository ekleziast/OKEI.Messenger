using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContextLibrary
{
    [Table("Message")]
    public class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; }

        [ForeignKey("Conversation")]
        public Guid ConversationID { get; set; }
        public Conversation Conversation { get; set; }

        [ForeignKey("Person")]
        public Guid PersonID { get; set; }
        public Person Person { get; set; }

        public string Text { get; set; }
    }
}
