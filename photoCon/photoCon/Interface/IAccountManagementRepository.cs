using photoCon.Models;

namespace photoCon.Interface
{
    public interface IAccountManagementRepository
    {
        bool VerifyInternalUser(string UserName, string Password);
        List<vm_UserDetails> GetInternalUserInfo(string empID);
        List<vm_UserDetails> GetSystemUserInfo(string UserName);
        List<vm_UserDetails> GetUsersList();
        List<AspNetUsers> GetEmployeeInfo(string empID);
        bool EnrollUser(AspNetUsers NewUser);
        bool AssignRole(AspNetUsers NewUser, string UserRole);
        bool UpdateUser(vm_UserDetails UpdateUser);
        bool DeleteUser(string DeleteUser);
        bool Save();
    }
}
