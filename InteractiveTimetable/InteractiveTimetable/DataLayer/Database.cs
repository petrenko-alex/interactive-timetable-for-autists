using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer;
using InteractiveTimetable.BusinessLayer.Contracts;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;
using SQLite.Extensions;
using SQLiteNetExtensions.Extensions;

namespace InteractiveTimetable.DataLayer
{
    class Database
    {
        private static object _locker = new object();

        private SQLiteConnection _connection;

        public Database(SQLiteConnection connection)
        {
            _connection = connection;

            _connection.CreateTable<User>();
            _connection.CreateTable<HospitalTrip>();
            _connection.CreateTable<Diagnostic>();
            _connection.CreateTable<CriterionGrade>();
            _connection.CreateTable<CriterionDefinition>();
            _connection.CreateTable<CriterionGradeType>();
            _connection.CreateTable<Schedule>();
            _connection.CreateTable<ScheduleItem>();
            _connection.CreateTable<Card>();
            _connection.CreateTable<CardType>();
        }

        public IEnumerable<T> GetItems<T>() where T : IBusinessEntity, new()
        {
            lock (_locker)
            {
                return (from i in _connection.Table<T>() select i).ToList();
            }
        }

        public IEnumerable<T> GetItemsCascade<T>()
            where T : IBusinessEntity, new()
        {
            lock (_locker)
            {
                return (from i in _connection.Table<T>()
                        select _connection.GetWithChildren<T>(i.Id, true)).
                        ToList();
            }
        }

        public T GetItem<T>(int id) where T : IBusinessEntity, new()
        {
            lock (_locker)
            {
                return _connection.Table<T>().FirstOrDefault(x => x.Id == id);
            }
        }

        public T GetItemCascade<T>(int id) where T : IBusinessEntity, new()
        {
            lock (_locker)
            {
                return _connection.GetWithChildren<T>(id, true);
            }
        }

        public int SaveItem<T>(T item) where T : IBusinessEntity
        {
            lock (_locker)
            {
                if (item.Id != 0)
                {
                    _connection.Update(item);
                    return item.Id;
                }

                _connection.Insert(item);
                return item.Id;
            }
        }

        public int SaveItemCascade<T>(T item) where T : IBusinessEntity
        {
            lock (_locker)
            {
                if (item.Id != 0)
                {
                    _connection.UpdateWithChildren(item);
                    return item.Id;
                }

                _connection.InsertWithChildren(item, true);
                return item.Id;
            }
        }

        public int DeleteItem<T>(int id) where T : IBusinessEntity
        {
            lock (_locker)
            {
                return _connection.Delete<T>(id);
            }
        }

        public void DeleteItemCascade<T>(T objectToDelete)
            where T : IBusinessEntity
        {
            lock (_locker)
            {
                _connection.Delete(objectToDelete, true);
            }
        }
    }
}