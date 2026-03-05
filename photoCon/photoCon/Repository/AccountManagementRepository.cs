using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Novell.Directory.Ldap;
using photoCon.Data;
using photoCon.Helper;
using photoCon.Interface;
using photoCon.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.DirectoryServices;
using System.Formats.Asn1;
using System.Linq;

namespace photoCon.Repository
{
    public class AccountManagementRepository : IAccountManagementRepository
    {
        private readonly ILogger<AccountManagementRepository> _logger;
        private readonly TallyProgramContext _tallyProgramContext;

        public AccountManagementRepository(ILogger<AccountManagementRepository> logger, TallyProgramContext tallyProgramContext) {
            _logger = logger;
            _tallyProgramContext = tallyProgramContext;
        }

        public string DomainDefault = "192.168.100.21"; //LDAP Default Domain IP
        public string DomainName = "CORPORATE\\"; //DirectoryServices Default Domain Name

        public bool VerifyInternalUser(string UserName, string Password) {
            //string UserName = userCredentials.UserName;
            //string Password = userCredentials.Password;

            bool isValidEmployee = false;

            try
            {
                var ldapConn = new LdapConnection();
                ldapConn.Connect(DomainDefault, 389);
                ldapConn.Bind(DomainName + UserName, Password);
                ////var test = ldapConn.Read("CN=FtreasuryHead1 H. L.TreasuryHead1,OU=TestUser,DC=newpagcor,DC=com").GetAttributeSet().GetAttribute("department").StringValue;
                ////var test = ldapConn.Search("OU=TestUser,DC=newpagcor,DC=com", 1, "CN=FtreasuryHead1 H. L.TreasuryHead1", ["*"], false);
                //var searcher = new DirectorySearcher();
                //searcher.Filter = ("sAMAccountName=" + UserName);
                //var searchResult = searcher.FindOne();
                //var resultProperties = searchResult.Properties;
                isValidEmployee = true;
                ldapConn.Disconnect();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                isValidEmployee = false; 
            }

            return isValidEmployee;
        }

        public List<vm_UserDetails> GetInternalUserInfo(string empID) {
            var LoggedInUser = new vm_UserCredentials { 
                UserName = "78-0001",
                Password = "Password01"
            };

            var UserInfo = new List<vm_UserDetails>();
            var UserDetails = new vm_UserDetails();

            if (VerifyInternalUser(LoggedInUser.UserName, LoggedInUser.Password) == true)
            {
                var ldapConn = new LdapConnection();
                ldapConn.Connect(DomainDefault, 389);
                ldapConn.Bind(DomainName + LoggedInUser.UserName, LoggedInUser.Password);
                try
                {
                    var searcher = new DirectorySearcher();
                    searcher.Filter = ("sAMAccountName=" + empID);
                    //var searchResult = searcher.FindOne().Properties["sAMAccountName"].ToString();
                    SearchResult searchResult = searcher.FindOne();

                    if (searchResult != null)
                    {
                        ResultPropertyCollection resultProperties = searchResult.Properties; //

                        UserDetails.UserName = resultProperties["sAMAccountName"][0].ToString().ToUpper();
                        UserDetails.FirstName = resultProperties["givenName"][0].ToString().ToUpper();
                        UserDetails.LastName = resultProperties["sn"][0].ToString().ToUpper();
                        UserDetails.MiddleName = resultProperties["initials"][0].ToString().ToUpper();
                        UserDetails.Department = resultProperties["Department"][0].ToString().ToUpper();
                        UserDetails.PayClass = resultProperties["Title"][0].ToString().ToUpper();
                        UserDetails.Email = resultProperties["userPrincipalName"][0].ToString().ToUpper();
                    }
                    else {
                        throw new Exception("");
                    }
                    
                }
                catch (Exception ex){
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                }

                ldapConn.Disconnect();
                UserInfo.Add(UserDetails);
            }

            return UserInfo;
        }

        public List<vm_UserDetails> GetSystemUserInfo(string UserName) {
            var UserDetails = _tallyProgramContext.AspNetUsers.Where(a => a.UserName == UserName).FirstOrDefault();
            var UserRoleID = _tallyProgramContext.AspNetUserRoles.Where(a => a.UserId == UserDetails.Id).FirstOrDefault();
            var UserRoleName = _tallyProgramContext.AspNetRoles.Where(a => a.Id == UserRoleID.RoleId).FirstOrDefault();

            var UserInfo = new vm_UserDetails {
                UserRole = UserRoleName.Name,
                UserName = UserDetails.UserName,
                LastName = UserDetails.LastName,
                FirstName = UserDetails.FirstName,
                MiddleName = UserDetails.MiddleName,
                Department = UserDetails.Department,
                PayClass = UserDetails.PayClass,
                Email = UserDetails.Email,
                Password = UserDetails.PasswordHash
            };

            var ReturnInfo = new List<vm_UserDetails> { UserInfo };

            return ReturnInfo;
        }



        public List<vm_UserDetails> GetUsersList() {
            var ActiveUsersList = _tallyProgramContext.AspNetUsers.ToList();

            var UsersList = _tallyProgramContext.AspNetUsers;
            var RolesList = _tallyProgramContext.AspNetRoles;
            var UserRoles = _tallyProgramContext.AspNetUserRoles;
            var CompleteList = (
                    from a in UserRoles
                    join b in UsersList on a.UserId equals b.Id
                    join c in RolesList on a.RoleId equals c.Id
                    select new vm_UserDetails() { 
                        UserRole = c.Name,
                        UserName = b.UserName,
                        FirstName = b.FirstName,
                        LastName = b.LastName,
                        MiddleName = b.MiddleName,
                        Department = b.Department,
                        PayClass = b.PayClass,
                        Email = b.Email,
                    }
                ).ToList();


            return CompleteList;
        }

        //Get Info from Active Directory (PAGCOR Employees)
        public List<AspNetUsers> GetEmployeeInfo(string empID) {
            var UserInformation = new List<AspNetUsers>().Where(a => a.Id=="").ToList();

            //Check UserID validity from AD

            //Testing Only
            var TestUser = new AspNetUsers();

            var UserName = "";
            var FirstName = "";
            var LastName = "";
            var MiddleInitial = "";
            var Deparment = "";
            var JobTitle = "";
            var EMail = "";

            switch (empID){
                case "00-0000":
                    UserName = "00-0000";
                    FirstName = "JUAN";
                    LastName = "DELA CRUZ";
                    MiddleInitial = "A";
                    Deparment = "TESTING DEPARTMENT";
                    JobTitle = "TESTER";
                    EMail = "JUAN.DELACRUZ@pagcor.ph";
                    break;
                case "00-0001":
                    UserName = "00-0001";
                    FirstName = "TESTER";
                    LastName = "DELA TEST";
                    MiddleInitial = "B";
                    Deparment = "TESTING DEPARTMENT";
                    JobTitle = "TESTER";
                    EMail = "TESTER.DELATEST@pagcor.ph";
                    break;
                case "00-0002":
                    UserName = "00-0002";
                    FirstName = "DREI";
                    LastName = "DELA DREI";
                    MiddleInitial = "C";
                    Deparment = "TESTING DEPARTMENT";
                    JobTitle = "TESTER";
                    EMail = "DREI.DELADREI@pagcor.ph";
                    break;
                default:
                    break;
            }

            if (UserName != "")
            {
                TestUser = new AspNetUsers
                {
                    UserName = UserName,
                    FirstName = FirstName,
                    LastName = LastName,
                    MiddleName = MiddleInitial,
                    Department = Deparment,
                    PayClass = JobTitle,
                    Email = EMail
                };

                UserInformation.Add(TestUser);
            }
            //

            return UserInformation;
        }

        public bool EnrollUser(AspNetUsers NewUser) {
            var passwordHasher = new PasswordHasher<AspNetUsers>();
            NewUser.NormalizedUserName = NewUser.UserName.ToString().ToUpper();
            NewUser.NormalizedEmail = NewUser.Email.ToString().ToUpper();
            NewUser.PasswordHash = passwordHasher.HashPassword(NewUser, NewUser.PasswordHash.ToString());

            _tallyProgramContext.Add(NewUser);
            return Save();
        }

        public bool AssignRole(AspNetUsers NewUser, string UserRole) {
            var RoleId = _tallyProgramContext.AspNetRoles.Where(a => a.Name == UserRole.Trim().ToUpper()).FirstOrDefault().Id;
            var AddRole = new AspNetUserRoles
            {
                UserId = NewUser.Id,
                RoleId = RoleId
            };
            _tallyProgramContext.Add(AddRole);
            return Save();
        }

        public bool UpdateUser(vm_UserDetails UpdateUser) {
            string[] DoNotUpdate = { "duhatwo2", "11-0124", "juanone1", "18-0404", "02-0555" }; //UserNames
            if (DoNotUpdate.Contains(UpdateUser.UserName) == false) {
                var SelectedUser = _tallyProgramContext.AspNetUsers.Where(a => a.UserName == UpdateUser.UserName).FirstOrDefault();
                var UserRole = _tallyProgramContext.AspNetRoles.Where(a => a.Id == UpdateUser.UserRole).FirstOrDefault();
                var SelectedUserRole = _tallyProgramContext.AspNetUserRoles.Where(a => a.UserId == SelectedUser.Id).FirstOrDefault();
                

                SelectedUser.FirstName = UpdateUser.FirstName;
                SelectedUser.LastName = UpdateUser.LastName;
                SelectedUser.MiddleName = UpdateUser.MiddleName;
                SelectedUser.Department = UpdateUser.Department;
                SelectedUser.PayClass = UpdateUser.PayClass;
                SelectedUser.Email = UpdateUser.Email;
                //SelectedUserRole.RoleId = UserRole.Id;
                //SelectedUser.PasswordHash = "";
            }

            //Only Judge User can be updated
            return Save();
            //return true;
        }

        public bool DeleteUser(string DeleteUser) {
            //string[] DoNotDelete = { "12cb38a5-b873-409d-b44a-365deb9b4239", "600fa799-6c63-48ca-a2ee-628b7abdd9f8", "64507e27-f151-4bb2-ad0c-8983bee1e799", "78115116-d473-4c51-a840-17216a1215e6", "a08cb68e-39fa-4861-88a6-31518a37785e" };
            string[] DoNotDelete = { "18-0404" }; //UserNames
            if (DoNotDelete.Contains(DeleteUser) == false)
            {
                var SelectedUser = _tallyProgramContext.AspNetUsers.Where(a => a.UserName == DeleteUser).FirstOrDefault();
                var SelectedUserRole = _tallyProgramContext.AspNetUserRoles.Where(a => a.UserId == SelectedUser.Id).FirstOrDefault();
                _tallyProgramContext.Remove(SelectedUser);
                _tallyProgramContext.Remove(SelectedUserRole);
                return Save();
            }
            else {
                return false;
            }
            
        }

        public bool Save() {
            return _tallyProgramContext.SaveChanges() > 0;
        }

    }
}
