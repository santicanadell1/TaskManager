﻿using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain
{
    public class User
    {
        private string firstName;
        private string lastName;
        private string email;
        private DateTime birthday;
        public string password;
        public List<Rol> roles = new List<Rol>();

        public string FirstName
        {
            get => firstName;
            set { firstName = string.IsNullOrWhiteSpace(value) ? 
                throw new UserFirstNameException() : value; }
        }

        public string LastName
        {
            get => lastName;
            set
            {
                lastName = string.IsNullOrWhiteSpace(value) ? 
                    throw new UserLastNameException() : 
                    value;
            }
        }

        public string Email
        {
            get => email;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new UserEmailException();
                }

                email = IsValidEmail(value) ? value : 
                    throw new UserEmailException();
            }
        }

        public DateTime Birthday
        {
            get => birthday;
            set { birthday = DateTime.Today < value ? 
                throw new UserBirthdayException() : value; }
        }

        public string Password
        {
            get => password;
            set { password = string.IsNullOrWhiteSpace(value) ? 
                throw new UserPasswordException() : value; }
        }

        public List<Rol> Roles
        {
            get => roles;
            set
            {
                if (value == null)
                {
                    throw new UserRolesInvalidAssignmentException();
                }

                roles = value;
            }
        }

        public User(string firstName, string lastName, string email, DateTime birthday, string password)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Email = email;
            this.Birthday = birthday;
            this.Password = password;
        }
        
        public User(){}

        private bool IsValidEmail(string email)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
        }


        public void AddRol(Rol rol)
        {
            if (roles.Contains(rol))
            {
                throw new UserRoleAlreadyExistsException(rol.ToString());
            }

            roles.Add(rol);
        }

        public void RemoveRol(Rol rol)
        {
            if (!roles.Contains(rol))
            {
                throw new UserRoleNotFoundException(rol.ToString());
            }

            roles.Remove(rol);
        }
    }
}