namespace RestAPI.Model
{
    public class ListPersonDto
    {
        public string PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IdDeleted { get; set; }
        public string DeleteReason { get; set; }
    }
}
