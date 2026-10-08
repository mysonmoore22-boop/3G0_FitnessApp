namespace FitnessApp.Models
{
    public class TrainerViewModelcs
    {
        public int ID { get; set; }
        public int UserID   { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Specialization { get; set; }  

    }
}
