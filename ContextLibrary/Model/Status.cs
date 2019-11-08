using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContextLibrary
{
    [Table("Statuses")]
    public class Status
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
