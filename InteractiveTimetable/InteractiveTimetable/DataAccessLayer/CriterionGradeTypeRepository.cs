﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    class CriterionGradeTypeRepository : BaseRepository
    {
        public CriterionGradeTypeRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public CriterionGradeType GetCriterionGradeType(int id)
        {
            return _database.GetItem<CriterionGradeType>(id);
        }

        public IEnumerable<CriterionGradeType> GetCriterionGradeTypes()
        {
            return _database.GetItems<CriterionGradeType>();
        }

        public int SaveCriterionGradeType(CriterionGradeType criterionGradeType)
        {
            return _database.SaveItem(criterionGradeType);
        }

        public int DeleteCriterionGradeType(int id)
        {
            return _database.DeleteItem<CriterionGradeType>(id);
        }
    }
}
