using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using photoCon.Models;
using photoCon.Interface;
using photoCon.Helper;
using Serilog;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace photoCon.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly ILogger<EnrollmentController> _logger;
        private readonly IAccountManagementRepository _accountManagementRepository;
        private readonly IAuditLogsRepository _auditLogsRepository;

        public EnrollmentController(ILogger<EnrollmentController> logger, IAccountManagementRepository enrollmentRepository, IAuditLogsRepository auditLogsRepository)
        {
            _logger = logger;
            _accountManagementRepository = enrollmentRepository;
            _auditLogsRepository = auditLogsRepository;
        }


        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnrollNewUser([FromBody] vm_UserDetails newUserData)
        {
            var ProcessStatus = 0;
            string ProcessMessage = "";
            string logDescription = "";
            string invalidInputs = "";
            string blankInputs = "";
            try
            {
                //Check if UserName(specifically) is Empty
                if (string.IsNullOrEmpty(newUserData.UserName.Trim()))
                {
                    throw new Exception("3");
                }

                //Check if UserName already exists
                bool isSystemUser = _accountManagementRepository.GetUsersList().Where(a => a.UserName == newUserData.UserName).Count() > 0;
                if (isSystemUser)
                {
                    throw new Exception("6");
                }

                //Set New Enrollee Details
                var NewEnrollment = new AspNetUsers();
                bool isPAGCORID = false;

                string? RoleName = newUserData.UserRole;
                if (RoleName == "JUDGE")
                {
                    NewEnrollment.UserName = newUserData.UserName;
                    NewEnrollment.FirstName = newUserData.FirstName;
                    NewEnrollment.LastName = newUserData.LastName;
                    NewEnrollment.MiddleName = newUserData.MiddleName;
                    NewEnrollment.Department = "NA"; //Non-PAGCOR Users have no Department Value. NA used as default placeholder
                    NewEnrollment.Email = newUserData.Email;
                    NewEnrollment.PasswordHash = newUserData.Password;
                }
                else if (RoleName == "ECDUSER")
                {
                    var PAGCOR_EmployeeInfo = _accountManagementRepository.GetInternalUserInfo(newUserData.UserName);

                    //Check if PAGCOR User AND PAGCOR UserName is not Empty
                    if (PAGCOR_EmployeeInfo.Count > 0 && PAGCOR_EmployeeInfo.FirstOrDefault().UserName != "")
                    {
                        var VerifiedUser = PAGCOR_EmployeeInfo.FirstOrDefault();
                        isPAGCORID = true;

                        NewEnrollment.UserName = VerifiedUser.UserName;
                        NewEnrollment.FirstName = VerifiedUser.FirstName;
                        NewEnrollment.LastName = VerifiedUser.LastName;
                        NewEnrollment.MiddleName = VerifiedUser.MiddleName;
                        NewEnrollment.Department = VerifiedUser.Department;
                        NewEnrollment.Email = VerifiedUser.Email;
                        NewEnrollment.PasswordHash = "Password01"; //Placeholder Value for now
                    }
                    else
                    {
                        throw new Exception("2");
                    }
                }

                //Check for Input Validation
                var InputValid = new InputValidator();
                var SanitizeInput = new SanitizedParam();
                var test = SanitizeInput.RemoveSpecialCharacters("test.test");

                //--Input Format
                var isUserNameValid = InputValid.IsValidUsername(NewEnrollment.UserName.Trim());
                var isPasswordValid = InputValid.IsValidPassword(NewEnrollment.PasswordHash.Trim());
                var isFirstNameValid = InputValid.IsValidName(NewEnrollment.FirstName.Trim());
                var isLastNameValid = InputValid.IsValidName(NewEnrollment.LastName.Trim());
                var isMiddleNameValid = InputValid.IsValidName(NewEnrollment.MiddleName.Trim());
                var isDepartmentValid = true; //No format validation for Department
                var isEmailValid = InputValid.IsValidEmail(NewEnrollment.Email.Trim());
                if (isPAGCORID && RoleName == "ECDUSER")
                {
                    isUserNameValid = true;
                    isPasswordValid = true;
                    isEmailValid = true;
                    ////For Test Users Only
                    //isFirstNameValid = true;
                    //isLastNameValid = true;
                }

                //--Is Input Null/Empty
                var isUserNameEmpty = InputValid.IsNullString(NewEnrollment.UserName.Trim());
                var isPasswordEmpty = InputValid.IsNullString(NewEnrollment.PasswordHash.Trim());
                var isFirstNameEmpty = InputValid.IsNullString(NewEnrollment.FirstName.Trim());
                var isLastNameEmpty = InputValid.IsNullString(NewEnrollment.LastName.Trim());
                var isMiddleNameEmpty = InputValid.IsNullString(NewEnrollment.MiddleName.Trim());
                var isEmailEmpty = InputValid.IsNullString(NewEnrollment.Email.Trim());
                var isDepartmentEmpty = false;
                if (isPAGCORID && RoleName == "ECDUSER") {
                    isPasswordEmpty = false;
                    isDepartmentEmpty = InputValid.IsNullString(NewEnrollment.Department.Trim());
                }
                else {
                    //Special Condition: Allow nullable Middle Initial and Email, but still check for formatting if not null
                    if (isMiddleNameEmpty == true) {
                        isMiddleNameValid = true; //Empty is still considered an incorrect input. Set to True to bypass null input
                        isMiddleNameEmpty = false; //Bypass null input
                    }

                    if (isEmailEmpty == true) {
                        isEmailValid = true; //Empty is still considered an incorrect input. Set to True to bypass null input
                        isEmailEmpty = false; //Bypass null input
                    }
                }

                //Check if any Input has invalid format
                if (!isUserNameValid || !isFirstNameValid || !isLastNameValid || !isMiddleNameValid || !isDepartmentValid || !isEmailValid || !isPasswordValid)
                {
                    ProcessMessage = ProcessMessage + (!isUserNameValid ? "*[Username] must have only letter and numbers with no spaces<br>" : "");
                    ProcessMessage = ProcessMessage + (!isFirstNameValid ? "*[First Name] must only have letters with spaces allowed<br>" : "");
                    ProcessMessage = ProcessMessage + (!isLastNameValid ? "*[Last Name] must only have letters with spaces allowed<br>" : "");
                    ProcessMessage = ProcessMessage + (!isMiddleNameValid ? "*[Middle Initial] can only have only up to two(2) letters<br>" : "");
                    ProcessMessage = ProcessMessage + (!isDepartmentValid ? "*--<br>" : "");
                    ProcessMessage = ProcessMessage + (!isEmailValid ? "*Invalid Email Address Format<br>" : "");
                    ProcessMessage = ProcessMessage + (!isPasswordValid ? "*[Password] must have only letter and numbers with no spaces<br>" : "");
                    throw new Exception("4");
                }
                //Check if any Input is Empty/Null
                else if (isUserNameEmpty || isFirstNameEmpty || isLastNameEmpty || isMiddleNameEmpty || isDepartmentEmpty || isEmailEmpty || isPasswordEmpty)
                {
                    throw new Exception("5");
                }


                //Save Enrollment + Assign UserRole
                _accountManagementRepository.EnrollUser(NewEnrollment);
                _accountManagementRepository.AssignRole(NewEnrollment, RoleName);

                //Generate Log
                logDescription = "Success | Enroll User | [" + User.Claims.First().Value + "] successfully enrolled new user: [" + newUserData.UserName + "] as [" + newUserData.UserRole + "] user type | Execution Date: " + DateTime.Now.ToString();

                //Set ProcessStatus
                ProcessStatus = 1; //User Enrollment Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                if (ex.Message.ToString() == "2")
                {
                    logDescription = "Error | Enroll User | [" + User.Claims.First().Value + "] failed to enroll [" + newUserData.UserName + "]. Invalid PAGCOR ID Number | Execution Date: " + DateTime.Now.ToString();
                    ProcessStatus = 2; //Invalid PAGCOR ID
                }
                else if (ex.Message.ToString() == "3")
                {
                    logDescription = "Error | Enroll User | [" + User.Claims.First().Value + "] cannot enroll UserName with empty or null value | Execution Date: " + DateTime.Now.ToString();
                    ProcessStatus = 3; //Empty UserName
                }
                else if (ex.Message.ToString() == "4")
                {
                    logDescription = "Error | Enroll User | [" + User.Claims.First().Value + "] attempted to enroll user with invalid field format | Execution Date: " + DateTime.Now.ToString();
                    ProcessStatus = 4; //An Input has invalid format
                }
                else if (ex.Message.ToString() == "5")
                {
                    logDescription = "Error | Enroll User | [" + User.Claims.First().Value + "] failed to enroll[" + newUserData.UserName + "]. A required field was empty or null | Execution Date: " + DateTime.Now.ToString();
                    ProcessStatus = 5; //An Input cannot be empty
                }
                else if (ex.Message.ToString() == "6")
                {
                    logDescription = "Error | Enroll User | [" + User.Claims.First().Value + "] failed to enroll[" + newUserData.UserName + "]. User with the same UserName already exists | Execution Date: " + DateTime.Now.ToString();
                    ProcessStatus = 6; //Existing UserName
                }
                else
                {
                    logDescription = "Error | Enroll User | [" + User.Claims.First().Value + "] failed to enroll[" + newUserData.UserName + "]. Unknown error has been encountered | Execution Date: " + DateTime.Now.ToString();
                    ProcessStatus = 0; //Unknown Error
                }
            }

            //Logs
            _auditLogsRepository.SystemAuditLog("Controller", "EnrollmentController_EnrollNewUser", 0, logDescription, User.Claims.First().Value);

            return Json(new { ProcessStatus, ProcessMessage });
        }

        [Authorize]
        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUser([FromBody] vm_UserDetails userDetails)
        {
            var returnText = "";
            string logDescription = "";
            try
            {
                bool isUpdated = _accountManagementRepository.UpdateUser(userDetails);
                if (isUpdated)
                {
                    returnText = "User has been updated";
                    logDescription = "Success | Update User | [" + User.Claims.First().Value + "] has updated [" + userDetails.UserName + "] | Execution Date: " + DateTime.Now.ToString();
                }
                else
                {
                    returnText = "Cannot update this user";
                    logDescription = "Error | Update User | [" + User.Claims.First().Value + "] has failed to update [" + userDetails.UserName + "] | Execution Date: " + DateTime.Now.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                returnText = "Cannot update this user";
                logDescription = "Error | Update User | [" + User.Claims.First().Value + "] has failed to update [" + userDetails.UserName + "]. Unknown error encountered | Execution Date: " + DateTime.Now.ToString();
            }

            //Logs
            _auditLogsRepository.SystemAuditLog("Controller", "EnrollmentController_UpdateUser", 0, logDescription, User.Claims.First().Value);

            return Json(returnText);
        }

        [Authorize]
        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser([FromBody] vm_UserDetails userDetails)
        {
            var returnText = "";
            string logDescription = "";
            try
            {
                var UserName = userDetails.UserName;
                bool isDeleted = _accountManagementRepository.DeleteUser(UserName);

                if (isDeleted)
                {
                    logDescription = "Success | Delete User | [" + User.Claims.First().Value + "] has deleted [" + userDetails.UserName + "] | Execution Date: " + DateTime.Now.ToString();
                    returnText = "User has been deleted";
                }
                else
                {
                    logDescription = "Error | Delete User | [" + User.Claims.First().Value + "] has failed to delete [" + userDetails.UserName + "] | Execution Date: " + DateTime.Now.ToString();
                    returnText = "Cannot delete this user";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                logDescription = "Error | Delete User | [" + User.Claims.First().Value + "] has failed to delete [" + userDetails.UserName + "]. Unknown error encountered | Execution Date: " + DateTime.Now.ToString();
                returnText = ex.Message.ToString();
            }

            //Logs
            _auditLogsRepository.SystemAuditLog("Controller", "EnrollmentController_DeleteUser", 0, logDescription, User.Claims.First().Value);

            return Json(returnText);
        }

        [Authorize]
        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetEmployeeInfo()
        {
            var EmployeeID = Request.Form["UserName"];

            //var GetEmployeeInfo = _accountManagementRepository.GetEmployeeInfo(EmployeeID);
            var GetEmployeeInfo = _accountManagementRepository.GetInternalUserInfo(EmployeeID);

            if (GetEmployeeInfo.Count() > 0)
            {
                return Json(GetEmployeeInfo.First());
            }
            else
            {
                return Json("");
            }
        }

        [Authorize]
        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetUserInfo()
        {
            var UserName = Request.Form["UserName"];
            //var UserInfo = _accountManagementRepository.GetUsersList().Where(a => a.UserName == UserName);
            var UserInfo = _accountManagementRepository.GetSystemUserInfo(UserName);
            if (UserInfo.Count() > 0)
            {
                return Json(UserInfo.First());
            }
            else
            {
                return Json("");
            }
        }

        [Authorize]
        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetUsersList()
        {
            try
            {
                var UsersList = new List<vm_UserDetails>();
                string CurrentUser = User.Claims.First().Value;
                var UserRole = User.Claims.Where(a => a.Type == ClaimTypes.Role).Select(a => a.Value).FirstOrDefault();
                if (UserRole == "SYSADMIN" || UserRole == "APPADMIN")
                {
                    UsersList = _accountManagementRepository.GetUsersList().Where(a => a.UserRole == "ECDUSER" || a.UserRole == "JUDGE" && a.UserName != CurrentUser).ToList();
                }
                else if (UserRole == "ECDUSER")
                {
                    UsersList = _accountManagementRepository.GetUsersList().Where(a => a.UserRole == "JUDGE" && a.UserName != CurrentUser).ToList();
                }

                return Json(UsersList);
            }
            catch
            {
                return Json("No Data");
            }
        }

        [Authorize]
        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyUser([FromBody] vm_UserCredentials userCredentials)
        {
            bool isValidEmployee = _accountManagementRepository.VerifyInternalUser(userCredentials.UserName, userCredentials.Password);
            return Json(isValidEmployee);
        }
    }
}