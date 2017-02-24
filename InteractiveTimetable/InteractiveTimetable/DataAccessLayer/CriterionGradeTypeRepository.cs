using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    public class CriterionGradeTypeRepository : BaseRepository
    {
        private const string PointTypeName = "POINT_GRADE";
        private const string TickTypeName = "TICK_GRADE";

        private static readonly List<int> PointTypeNumbers = new List<int>()
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17
        };

        private static readonly List<int> TickTypeNumbers = new List<int>()
        {
            18
        };

        public CriterionGradeTypeRepository(SQLiteConnection connection)
            : base(connection)
        {
            /* Adding default criterion grade types */
            var pointCriterionGradeType = new CriterionGradeType()
            {
                TypeName = PointTypeName
            };

            var tickCriterionGradeType = new CriterionGradeType()
            {
                TypeName = TickTypeName
            };

            SaveCriterionGradeType(pointCriterionGradeType);
            SaveCriterionGradeType(tickCriterionGradeType);
        }

        public CriterionGradeType GetCriterionGradeType(int id)
        {
            return _database.GetItemCascade<CriterionGradeType>(id);
        }

        public IEnumerable<CriterionGradeType> GetCriterionGradeTypes()
        {
            return _database.GetItemsCascade<CriterionGradeType>();
        }

        private int SaveCriterionGradeType(CriterionGradeType criterionGradeType)
        {
            return _database.SaveItemCascade(criterionGradeType);
        }

        public int DeleteCriterionGradeType(int id)
        {
            return _database.DeleteItem<CriterionGradeType>(id);
        }

        public void DeleteCriterionGradeTypeCascade(CriterionGradeType type)
        {
            _database.DeleteItemCascade(type);
        }

        public CriterionGradeType GetCriterionGradeTypeByNumber(int number)
        {
            if (PointTypeNumbers.Contains(number))
            {
                return GetPointCriterionGradeType();
            }
            else if (TickTypeNumbers.Contains(number))
            {
                return GetTickCriterionGradeType();
            }
            else
            {
                throw new ArgumentException("Not a valid number.");
            }
        }

        public CriterionGradeType GetPointCriterionGradeType()
        {
            return GetCriterionGradeTypes().
                    FirstOrDefault(type => type.TypeName == PointTypeName);
        }

        public CriterionGradeType GetTickCriterionGradeType()
        {
            return GetCriterionGradeTypes().
                    FirstOrDefault(type => type.TypeName == TickTypeName);
        }
    }
}