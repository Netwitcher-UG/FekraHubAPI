using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string StreetNr { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }

        public ICollection<Room>  room { get; set; }
    }
}
