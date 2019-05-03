using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CmsShoppingCart.Models.Data
{
    [Table("tblUsers")]
    public class UserDTO
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }


        public string PA_Address { get; set; }
        public string PA_Suburb { get; set; }
        public string PA_State { get; set; }
        public string PA_PostCode { get; set; }
        public string PA_Country { get; set; }

        public string DA_Address { get; set; }
        public string DA_Suburb { get; set; }
        public string DA_State { get; set; }
        public string DA_PostCode { get; set; }
        public string DA_Country { get; set; }

    }
}