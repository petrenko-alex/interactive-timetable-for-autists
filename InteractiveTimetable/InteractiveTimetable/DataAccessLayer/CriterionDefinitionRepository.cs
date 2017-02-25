using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class CriterionDefinitionRepository : BaseRepository
    {
        private const int NumberOfCriterions = 18;

        private CriterionGradeTypeRepository _criterionGradeTypeRepository;

        public CriterionDefinitionRepository(SQLiteConnection connection)
            : base(connection)
        {
            _criterionGradeTypeRepository
                    = new CriterionGradeTypeRepository(connection);

            /* Adding default criterion definitions */
            for (int i = 1; i <= NumberOfCriterions; ++i)
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

        public int GetNumberOfCriterions()
        {
            return NumberOfCriterions;
        }

        public CriterionDefinition GetCriterionDefinitionByNumber(int number)
        {
            if (number <= 0 ||
                number > NumberOfCriterions)
            {
                throw new ArgumentException("Not a valid number.");
            }

            return GerCriterionDefinitions().
                FirstOrDefault(x => x.Number == number);

        }

        internal int SaveCriterionDefinition(
            CriterionDefinition criterionDefinition)
        {
            return _database.SaveItemCascade(criterionDefinition);
        }

        internal int DeleteCriterionDefinition(int id)
        {
            return _database.DeleteItem<CriterionDefinition>(id);
        }

        internal void DeleteCriterionDefinitionCascade(
            CriterionDefinition criterionDefinition)
        {
            _database.DeleteItemCascade(criterionDefinition);
        }
    }
}