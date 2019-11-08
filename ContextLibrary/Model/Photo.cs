using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContextLibrary
{
    [Table("Photos")]
    public class Photo
    {
        [ForeignKey("Person")]
        [Key]
        public Guid ID { get; set; }
        public byte[] PhotoSource { get; set; }
    }
}