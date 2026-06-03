using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeService.Models
{
    public class LeaveBalance
    {
        public int Id {get;set;}
        public string EmployeeId {get;set;}=string.Empty;
        public string LeaveType {get;set;}=string.Empty;
        public int TotalAllocated {get;set;}
         public int Used {get;set;}

         [NotMapped]
         public int Remaining => TotalAllocated-Used;
    }
    public static class LeaveTypes
    {
        public const string Casual="Casual";
        public const string Sick="Sick";
        public const string Privilege="Privilege";
        
        public static bool IsValid(string leaveType)=>
        leaveType is Casual or Sick or Privilege;
    }

}