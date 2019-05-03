using CmsShoppingCart.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsShoppingCart.Models.ViewModels.Account
{
    public class UserVM
    {
        public UserVM()
        {

        }

        public UserVM(UserDTO row)
        {
            Id = row.Id;
            FirstName = row.FirstName;
            LastName = row.LastName;
            EmailAddress = row.EmailAddress;
            Username = row.Username;
            Password = row.Password;

            PA_Address = row.PA_Address;
            PA_Suburb = row.PA_Suburb;
            PA_State = row.PA_State;
            PA_PostCode = row.PA_PostCode;
            PA_Country = row.PA_Country;

            DA_Address = row.DA_Address;
            DA_Suburb = row.DA_Suburb;
            DA_State = row.DA_State;
            DA_PostCode = row.DA_PostCode;
            DA_Country = row.DA_Country;
        }


        public int Id { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Postal Address")]
        public string PA_Address { get; set; }
        [Display(Name = "Postal Suburb")]
        public string PA_Suburb { get; set; }
        [Display(Name = "Postal State")]
        public string PA_State { get; set; }
        [Display(Name = "Postal PostCode")]
        public string PA_PostCode { get; set; }
        [Display(Name = "Postal Country")]
        public string PA_Country { get; set; }

        [Display(Name = "Delivery Address")]
        public string DA_Address { get; set; }
        [Display(Name = "Delivery Suburb")]
        public string DA_Suburb { get; set; }
        [Display(Name = "Delivery State")]
        public string DA_State { get; set; }
        [Display(Name = "Postal PostCode")]
        public string DA_PostCode { get; set; }
        [Display(Name = "Postal Country")]
        public string DA_Country { get; set; }
    }
}