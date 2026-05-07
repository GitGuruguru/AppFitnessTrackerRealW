using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppFitnessTrackerReal.Models
{
    internal class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _name;
        public string Name {
            get => string.IsNullOrEmpty(_name) ? "Erroe: String is not Stringing" : _name;
            set => _name = value;
        }

        private string _password;

        public string Password { 
            get => string.IsNullOrEmpty(_password) ? "Error: Password is not Passwording" : _password;
            set => _password = value;
        }
        private string _email;
        public string Email
        {
            get => string.IsNullOrEmpty(_email) ? "email" : _email;
            set => _email = value;
        }
   
        //public User( string name, string password)
        //{
        //    name = Name;
        //    password = Password;
        //}
        //public  string ReturnBasicInfo()
        //{
        //    return $"UserName {Name}, Password {Password}";
        //}
    }
}
