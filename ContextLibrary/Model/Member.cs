using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContextLibrary
{
    [Table("Member")]
    public class Member
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Person")]
        public Guid PersonID { get; set; }
        public Person Person { get; set; }

        [ForeignKey("Conversation")]
        public Guid ConversationID { get; set; }
        public Conversation Conversation { get; set; }
    }
}
