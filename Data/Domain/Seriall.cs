using System.ComponentModel.DataAnnotations;

namespace TES.Data.Domain
{
    public class Seriall
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string SerialNo { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        [StringLength(50)]
        public string DeviceType { get; set; }
        [StringLength(50)]
        public string DeviceModel { get; set; }

    }
}
