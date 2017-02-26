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

        private CriterionGradeTypeRepository _gradeTypes;

        public CriterionDefinitionRepository(SQLiteConnection connection)
            : base(connection)
        {
            _gradeTypes = new CriterionGradeTypeRepository(connection);

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
                    CriterionGradeTypeId = _gradeTypes.
                            GetCriterionGradeTypeByNumber(i).Id
                };
                SaveCriterionDefinition(criterionDefinition);
            }
        }

        public CriterionDefinition GetCriterionDefinition(int criterionId)
        {
            return _database.GetItemCascade<CriterionDefinition>(criterionId);
        }

        public IEnumerable<CriterionDefinition> GerCriterionDefinitions()
        {
            return _database.GetItemsCascade<CriterionDefinition>().
                             OrderBy(x => x.Number);
        }

        public int GetNumberOfCriterions()
        {
            return NumberOfCriterions;
        }

        public CriterionGradeType GetCriterionGradeType(int criterionId)
        {
            var criterion = GetCriterionDefinition(criterionId);
            if (criterion != null)
            {
                int gradeTypeId = criterion.CriterionGradeTypeId;

                return _gradeTypes.GetCriterionGradeType(gradeTypeId);
            }

            return null;
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

        public bool IsPointGradeTypeCriterion(int criterionId)
        {
            var gradeType = GetCriterionGradeType(criterionId);
            return _gradeTypes.IsPointGradeType(gradeType);
        }

        public bool IsTickGradeTypeCriterion(int criterionId)
        {
            var gradeType = GetCriterionGradeType(criterionId);
            return _gradeTypes.IsTickGradeType(gradeType);
        }

        internal int SaveCriterionDefinition(
            CriterionDefinition criterionDefinition)
        {
            return _database.SaveItemCascade(criterionDefinition);
        }

        internal int DeleteCriterionDefinition(int criterionId)
        {
            return _database.DeleteItem<CriterionDefinition>(criterionId);
        }

        internal void DeleteCriterionDefinitionCascade(
            CriterionDefinition criterion)
        {
            _database.DeleteItemCascade(criterion);
        }
    }
}