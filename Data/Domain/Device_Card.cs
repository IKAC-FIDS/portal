using System.ComponentModel.DataAnnotations;

namespace TES.Data.Domain
{
    public class Device_Card
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string TypeDevice { get; set; }
        [StringLength(50)]
        public string TypeCard { get; set; }
    }
}
