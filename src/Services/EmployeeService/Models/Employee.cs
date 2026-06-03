using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeService.Models
{
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string EmployeeId {get;set;}
        public string Name {get;set;}=string.Empty;
        public string Email {get;set;}=string.Empty;
        public string Role {get;set;}=string.Empty;
         public string? ManagerId {get;set;}
         public DateTime CreatedAt {get;set;}
    }
}