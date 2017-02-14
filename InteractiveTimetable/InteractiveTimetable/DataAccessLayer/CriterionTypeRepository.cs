using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    class CriterionTypeRepository : BaseRepository
    {
        public CriterionTypeRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public CriterionType GetCriterionType(int id)
        {
            return _database.GetItem<CriterionType>(id);
        }

        public IEnumerable<CriterionType> GetCriterionTypes()
        {
            return _database.GetItems<CriterionType>();
        }

        public int SaveCriterionType(CriterionType criterionType)
        {
            return _database.SaveItem(criterionType);
        }

        public int DeleteCriterionType(int id)
        {
            return _database.DeleteItem<CriterionType>(id);
        }
    }
}
