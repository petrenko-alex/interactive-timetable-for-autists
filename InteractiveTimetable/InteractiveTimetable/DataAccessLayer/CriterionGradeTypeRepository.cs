using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class CriterionGradeTypeRepository : BaseRepository
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

        internal CriterionGradeTypeRepository(SQLiteConnection connection)
            : base(connection)
        {
            var types = GetCriterionGradeTypes().ToList();
            if (types.Count == 0)
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
        }

        internal CriterionGradeType GetCriterionGradeType(int id)
        {
            return _database.GetItemCascade<CriterionGradeType>(id);
        }

        internal IEnumerable<CriterionGradeType> GetCriterionGradeTypes()
        {
            return _database.GetItemsCascade<CriterionGradeType>();
        }

        internal int SaveCriterionGradeType(CriterionGradeType criterionGradeType)
        {
            return _database.SaveItemCascade(criterionGradeType);
        }

        internal int DeleteCriterionGradeType(int id)
        {
            return _database.DeleteItem<CriterionGradeType>(id);
        }

        internal void DeleteCriterionGradeTypeCascade(CriterionGradeType type)
        {
            _database.DeleteItemCascade(type);
        }

        internal CriterionGradeType GetCriterionGradeTypeByNumber(int number)
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
                throw new ArgumentException(
                    Resources.
                    Validation.
                    CriterionGradeTypeValidationStrings.
                    NotValidNumber);
            }
        }

        internal CriterionGradeType GetPointCriterionGradeType()
        {
            return GetCriterionGradeTypes().
                    FirstOrDefault(type => type.TypeName == PointTypeName);
        }

        internal CriterionGradeType GetTickCriterionGradeType()
        {
            return GetCriterionGradeTypes().
                    FirstOrDefault(type => type.TypeName == TickTypeName);
        }

        internal bool IsPointGradeType(CriterionGradeType gradeType)
        {
            if (gradeType != null)
            {
                return gradeType.TypeName.Equals(PointTypeName);
            }

            return false;
        }

        internal bool IsTickGradeType(CriterionGradeType gradeType)
        {
            if (gradeType != null)
            {
                return gradeType.TypeName.Equals(TickTypeName);
            }

            return false;
        }
    }
}