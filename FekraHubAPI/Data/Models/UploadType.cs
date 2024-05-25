using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class UploadType
    {
        [Key]
        public int Id { get; set; }
        public string TypeTitle { get; set; }

        public ICollection<Upload> Upload { get; set; }

        internal static Task<string> GetSingleOrDefaultAsync(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }
    }
}
