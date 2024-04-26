namespace Khairah_.Models
{
    public class IndexRecord
    {

        public IEnumerable<OurSponsor> Sponsors { get; set; }
        public IEnumerable<NgoVolunteer> Ngovolunteers { get; set; }
        public IEnumerable<Cause> Cause { get; set; }
        public IEnumerable<NgoEvent> Events { get; set; }

        public IEnumerable<Donation> Donations { get; set; }

        public IEnumerable<Contact> contacts { get; set; }

        public IEnumerable<Volunteer> volrequest { get; set; }

    }
}
