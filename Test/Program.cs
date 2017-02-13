using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Managers;
using InteractiveTimetable.BusinessLayer;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;


namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string dbPath = "../../Development/database.db3";
            var connection = new SQLiteConnection(dbPath);

            TestUserTable(connection);
        }

        private static void TestUserTable(SQLiteConnection connection)
        {
            UserManager userManager = new UserManager(connection);

            if (userManager.GetUsers().Count == 0)
            {
                Console.WriteLine("User table is empty");
                Console.WriteLine("Adding users...");

                /* Preparing data */
                User user1 = new User()
                {
                    FirstName = "Alexander",
                    LastName = "Petrenko",
                    PatronymicName = "Andreevich",
                    BirthDate = System.DateTime.Parse("25.07.1995").Date,
                    PhotoPath = "avatar1.jpg",
                    IsDeleted = false
                };

                User user2 = new User()
                {
                    FirstName = "Ivan",
                    LastName = "Ivanov",
                    PatronymicName = "Ivanovich",
                    BirthDate = System.DateTime.Parse("25.07.1995").Date,
                    PhotoPath = "avatar2.jpg",
                    IsDeleted = false
                };

                userManager.SaveUser(user1);
                userManager.SaveUser(user2);

                Console.WriteLine("Users were added.");
                Console.WriteLine("Reading users...\n");

                /* Reading users */
                var users = userManager.GetUsers();
                foreach (var i in users)
                {
                    Console.WriteLine(i.ToString());
                }

                Console.WriteLine("Modifying second user name from Ivan to IVAN...");
                var user = userManager.GetUser(2);
                user.FirstName = "IVAN";
                userManager.SaveUser(user);

                Console.WriteLine("Deleting first user.\n");
                userManager.DeleteUser(1);

                /* Reading users */
                users = userManager.GetUsers();
                foreach (var i in users)
                {
                    Console.WriteLine(i.ToString());
                }
            }
            else
            {
                Console.WriteLine("User Table is NOT empty");
                Console.WriteLine("Getting users...\n");
                
                /* Reading users */
                var users = userManager.GetUsers();
                foreach (var i in users)
                {
                    Console.WriteLine(i.ToString());
                }
            }
        }

        private static void TestHospitalTripTable(SQLiteConnection connection)
        {
            
        }
    }
}
