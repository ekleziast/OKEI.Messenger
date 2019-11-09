using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContextLibrary
{
    [Table("Photo")]
    public class Photo
    {
        [ForeignKey("Person")]
        [Key]
        public Guid ID { get; set; }
        public virtual Person Person { get; set; }
        public byte[] PhotoSource { get; set; }
    }
}