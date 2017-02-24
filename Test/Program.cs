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
        private static SQLiteConnection _connection;

        static void Main(string[] args)
        {
            string dbPath = "database.db3";
            _connection = new SQLiteConnection(dbPath);

            //TestUserTable();
            TestHospitalTripTable();
        }

        private static void TestUserTable()
        {
            UserManager userManager = new UserManager(_connection);

            if (!userManager.GetUsers().Any())
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

                Console.WriteLine("Modifying second user name " +
                                  "from Ivan to IVAN...");
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

        private static void TestHospitalTripTable()
        {
            HospitalTripManager hospitalTripManager = 
                new HospitalTripManager(_connection);
            UserManager userManager = 
                new UserManager(_connection);

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
            var userId = userManager.SaveUser(user1);

            /* Adding correct trips */
            HospitalTrip hospitalTrip1 = new HospitalTrip()
            {
                StartDate = DateTime.Parse("20.02.2017"),
                FinishDate = DateTime.Parse("25.02.2017"),
                UserId = userId
            };
            hospitalTripManager.SaveHospitalTrip(hospitalTrip1);
            Console.WriteLine("Trip in the future was added");

            HospitalTrip hospitalTrip2 = new HospitalTrip()
            {
                StartDate = DateTime.Parse("18.01.2017"),
                FinishDate = DateTime.Parse("19.02.2017"),
                UserId = userId
            };
            hospitalTripManager.SaveHospitalTrip(hospitalTrip2);
            Console.WriteLine("Current trip was added");

            /* Reading trips */
            var trips = hospitalTripManager.GetHospitalTrips(userId);
            foreach (var i in trips)
            {
                Console.WriteLine(i.ToString());
            }


            /* Adding incorrect trips 
             Start date later than finish date 
            hospitalTrip1 = new HospitalTrip()
            {
                StartDate = DateTime.Parse("25.02.2017"),
                FinishDate = DateTime.Parse("18.02.2017"),
                UserId = userId
            };
            hospitalTripManager.SaveHospitalTrip(hospitalTrip1);

             User is not set 
            hospitalTrip1 = new HospitalTrip()
            {
                StartDate = DateTime.Parse("25.02.2017"),
                FinishDate = DateTime.Parse("18.02.2017"),
            };
            hospitalTripManager.SaveHospitalTrip(hospitalTrip1);*/
        }
    }
}