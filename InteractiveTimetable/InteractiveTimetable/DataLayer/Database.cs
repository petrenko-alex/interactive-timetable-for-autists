using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer;
using InteractiveTimetable.BusinessLayer.Contracts;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataLayer
{
    class Database
    {
        private static object _locker = new object();

        private SQLiteConnection _database;

        public Database(SQLiteConnection connection)
        {
            _database = connection;
            
            _database.CreateTable<User>();
            _database.CreateTable<HospitalTrip>();
            _database.CreateTable<Diagnostic>();
            _database.CreateTable<CriterionGrade>();
            _database.CreateTable<CriterionDefinition>();
            _database.CreateTable<CriterionGradeType>();
            _database.CreateTable<Schedule>();
            _database.CreateTable<ScheduleItem>();
            _database.CreateTable<Card>();
            _database.CreateTable<CardType>();
        }

        public IEnumerable<T> GetItems<T>() where T : IBusinessEntity, new()
        {
            lock (_locker)
            {
                return (from i in _database.Table<T>() select i).ToList();
            }
        }

        public T GetItem<T>(int id) where T : IBusinessEntity, new()
        {
            lock (_locker)
            {
                return _database.Table<T>().FirstOrDefault(x => x.Id == id);
            }
        }

        public int SaveItem<T>(T item) where T : IBusinessEntity
        {
            lock (_locker)
            {
                if (item.Id != 0)
                {
                    _database.Update(item);
                    return item.Id;
                }
                else
                {
                    return _database.Insert(item);
                }
            }
        }

        public int DeleteItem<T>(int id) where T : IBusinessEntity
        {
            lock (_locker)
            {
                return _database.Delete<T>(id);
            }
        }

    }
}
