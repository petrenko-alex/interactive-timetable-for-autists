using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    class CriterionDefinitionRepository : BaseRepository
    {
        public CriterionDefinitionRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public CriterionDefinition GetCriterionDefinition(int id)
        {
            return _database.GetItem<CriterionDefinition>(id);
        }

        public IEnumerable<CriterionDefinition> GerCriterionDefinitions()
        {
            return _database.GetItems<CriterionDefinition>();
        }

        public int SaveCriterionDefinition(CriterionDefinition criterionDefinition)
        {
            return _database.SaveItem(criterionDefinition);
        }

        public int DeleteCriterionDefinition(int id)
        {
            return _database.DeleteItem<CriterionDefinition>(id);
        }
    }
}
