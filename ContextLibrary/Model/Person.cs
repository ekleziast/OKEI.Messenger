using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContextLibrary
{
    [Table("Persons")]
    public class Person
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; }

        public string Name { get; set; }
        public string SurName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        [ForeignKey("Status")]
        public int? StatusID { get; set; }
        public Status Status { get; set; }

        [ForeignKey("Photo")]
        public Guid? PhotoID { get; set; }
        public Photo Photo { get; set; }
    }
}
