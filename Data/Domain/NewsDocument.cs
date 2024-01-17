using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("NewsDocument")]
    public class NewsDocument
    {
        public int Id { get; set; }
        public int NewsId { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public virtual News News { get; set; }
    }
}