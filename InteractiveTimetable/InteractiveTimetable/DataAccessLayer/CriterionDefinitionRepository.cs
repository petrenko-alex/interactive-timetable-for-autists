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

        internal CriterionDefinitionRepository(SQLiteConnection connection)
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

        internal CriterionDefinition GetCriterionDefinition(int criterionId)
        {
            return _database.GetItemCascade<CriterionDefinition>(criterionId);
        }

        internal IEnumerable<CriterionDefinition> GerCriterionDefinitions()
        {
            return _database.GetItemsCascade<CriterionDefinition>().
                             OrderBy(x => x.Number);
        }

        internal int GetNumberOfCriterions()
        {
            return NumberOfCriterions;
        }

        internal CriterionGradeType GetCriterionGradeType(int criterionId)
        {
            var criterion = GetCriterionDefinition(criterionId);
            if (criterion != null)
            {
                int gradeTypeId = criterion.CriterionGradeTypeId;

                return _gradeTypes.GetCriterionGradeType(gradeTypeId);
            }

            return null;
        }

        internal CriterionDefinition GetCriterionDefinitionByNumber(int number)
        {
            if (number <= 0 ||
                number > NumberOfCriterions)
            {
                throw new ArgumentException(
                    Resources.
                    Validation.
                    CriterionDefinitionValidationStrings.
                    NotValidNumber);
            }

            return GerCriterionDefinitions().
                    FirstOrDefault(x => x.Number == number);
        }

        internal CriterionDefinition GetCriterionDefinitionByDefinition(
            string definition)
        {
            var criterionDefinition = GerCriterionDefinitions().
                FirstOrDefault(x => x.Definition == definition);

            if (criterionDefinition == null)
            {
                throw new ArgumentException(
                    Resources.
                    Validation.
                    CriterionDefinitionValidationStrings.
                    NotValidDefinition);
            }

            return criterionDefinition;
        }

        internal bool IsPointGradeTypeCriterion(int criterionId)
        {
            var gradeType = GetCriterionGradeType(criterionId);
            return _gradeTypes.IsPointGradeType(gradeType);
        }

        internal bool IsTickGradeTypeCriterion(int criterionId)
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