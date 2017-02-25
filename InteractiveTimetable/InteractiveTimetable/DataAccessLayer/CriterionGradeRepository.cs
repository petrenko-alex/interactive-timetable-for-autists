using System;
using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class CriterionGradeRepository : BaseRepository
    {
        private CriterionDefinitionRepository _criterionDefinitionRepository;

        private const int MinimumPointGrade = 1;
        private const int MaximumPointGrade = 4;
        private const int MaximumLengthOfTickGradeString = 4;

        public CriterionGradeRepository(SQLiteConnection connection)
            : base(connection) {}

        public CriterionGrade GetCriterionGrade(int id)
        {
            return _database.GetItem<CriterionGrade>(id);
        }

        public IEnumerable<CriterionGrade> GetCriterionGrades()
        {
            return _database.GetItems<CriterionGrade>();
        }

        public int SaveCriterionGrade(CriterionGrade grade)
        {
            /* Data validation */
            Validate(grade);

            return _database.SaveItem(grade);
        }

        public int DeleteCriterion(int id)
        {
            return _database.DeleteItem<CriterionGrade>(id);
        }

        public CriterionGradeType GetCriterionGradeType(CriterionGrade grade)
        {
            var criterionId = grade.CriterionDefinitionId;
            var criterion = _criterionDefinitionRepository.
                    GetCriterionDefinition(criterionId);

            return _criterionDefinitionRepository.
                    GetCriterionGradeType(criterion);
        }

        private void Validate(CriterionGrade grade)
        {
            var criterionId = grade.CriterionDefinitionId;
            var criterion = _criterionDefinitionRepository.
                    GetCriterionDefinition(criterionId);

            bool isPointGradeType = _criterionDefinitionRepository.
                    IsPointGradeTypeCriterion(criterion);

            bool isTickGradeType = _criterionDefinitionRepository.
                    IsTickGradeTypeCriterion(criterion);

            if (isPointGradeType)
            {
                int numberGrade;

                if (int.TryParse(grade.Grade, out numberGrade))
                {
                    if (numberGrade < MinimumPointGrade ||
                        numberGrade > MaximumPointGrade)
                    {
                        throw new ArgumentException("Not valid grade.");
                    }
                }
                else
                {
                    throw new ArgumentException("Not valid grade.");
                }
            }
            else if (isTickGradeType)
            {
                if (grade.Grade.Length < 1 ||
                    grade.Grade.Length > MaximumLengthOfTickGradeString)
                {
                    throw new ArgumentException("Not valid grade.");
                }
            }
            else
            {
                throw new ArgumentException("Not valid grade type.");
            }
        }
    }
}