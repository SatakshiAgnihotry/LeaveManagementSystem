using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserId {get;set;}

        public string Email {get;set;}

        public string PasswordHash {get;set;}

        public string Name {get;set;}
        public string Role {get;set;}
         public string? ManagerId {get;set;}
    }
}