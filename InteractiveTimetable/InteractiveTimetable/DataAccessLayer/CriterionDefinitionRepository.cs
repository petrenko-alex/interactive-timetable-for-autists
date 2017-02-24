using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    public class CriterionDefinitionRepository : BaseRepository
    {
        private static readonly int NUMBER_OF_CRITERIONS = 18;
        private CriterionGradeTypeRepository _criterionGradeTypeRepository;

        public CriterionDefinitionRepository(SQLiteConnection connection)
            : base(connection)
        {
            _criterionGradeTypeRepository
                    = new CriterionGradeTypeRepository(connection);

            /* Adding default criterion definitions */
            for (int i = 1; i <= NUMBER_OF_CRITERIONS; ++i)
            {
                /* Creating a criterion */
                var resourceString = "Criterion" + i;
                var criterionDefinition = new CriterionDefinition()
                {
                    Number = i,
                    Definition = Resources.
                            Repositories.
                            CriterionDefinitionStrings.
                            ResourceManager.
                            GetString(resourceString),
                    CriterionGradeTypeId = _criterionGradeTypeRepository.
                            GetCriterionGradeTypeByNumber(i).Id
                };
                SaveCriterionDefinition(criterionDefinition);
            }
        }

        public CriterionDefinition GetCriterionDefinition(int id)
        {
            return _database.GetItemCascade<CriterionDefinition>(id);
        }

        public IEnumerable<CriterionDefinition> GerCriterionDefinitions()
        {
            return _database.GetItemsCascade<CriterionDefinition>();
        }

        public int SaveCriterionDefinition(
            CriterionDefinition criterionDefinition)
        {
            return _database.SaveItemCascade(criterionDefinition);
        }

        public int DeleteCriterionDefinition(int id)
        {
            return _database.DeleteItem<CriterionDefinition>(id);
        }
    }
}