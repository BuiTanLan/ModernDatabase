using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.OrderNeo4j
{
    public class UserNeo4j
    {
        public UserNeo4j()
        {

        }

        public UserNeo4j(string mail)
        {
            BuyerEmail = mail;
        }

        public UserNeo4j(AppUser user)
        {
            BuyerEmail = user.Email;
            FirstName = user.Address.FirstName;
            LastName = user.Address.LastName;
            Street = user.Address.Street;
            City = user.Address.City;
            State = user.Address.State;
            ZipCode = user.Address.ZipCode;
        }

        public UserNeo4j(Address u)
        {
            BuyerEmail = u.AppUser.Email;
            FirstName = u.FirstName;
            LastName = u.LastName;
            Street = u.Street;
            City = u.City;
            State = u.State;
            ZipCode = u.ZipCode;
        }
        public string BuyerEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}
