using System;

namespace LegacyApp
{
    public class UserService
    {
        private bool AreCridentialsCorrect(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }
            return true;
        }

        private bool IsEmailCorrect(string email)
        {
            if (email.Contains("@") && email.Contains("."))
            {
                return true;
            }

            return false;
        }

        private bool IsOver21(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }

            return true;
        }

        private User makeUserForClient(int clientId,DateTime dateOfBirth,string email,string firstName, string lastName)
        {
            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            
            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (client.Type == "ImportantClient")
            {
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    creditLimit *= 2;
                    user.CreditLimit = creditLimit;
                }
            }
            else
            {
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }

            return user;
        }

        private bool IsUserCreditLimited(User user)
        {
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return true;
            }

            return false;
        }
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!AreCridentialsCorrect(firstName,lastName))
            {
                return false;
            }

            if (!IsEmailCorrect(email))
            {
                return false;
            }
            if (!IsOver21(dateOfBirth))
            {
                return false;
            }
            
            var user = makeUserForClient(clientId, dateOfBirth, email, firstName, lastName);
            
            if (IsUserCreditLimited(user))
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }
    }
}
