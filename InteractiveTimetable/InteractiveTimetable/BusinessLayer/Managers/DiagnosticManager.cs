using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using SQLite;

namespace InteractiveTimetable.BusinessLayer.Managers
{
    public class DiagnosticManager
    {
        private readonly DiagnosticRepository _repository;

        public DiagnosticManager(SQLiteConnection connection)
        {
            _repository = new DiagnosticRepository(connection);
        }

        public Diagnostic GetDiagnostic(int diagnosticId)
        {
            return _repository.GetDiagnostic(diagnosticId);
        }

        public IEnumerable<Diagnostic> GetDiagnostics(int hospitalTripId)
        {
            return _repository.GetTripDiagnostics(hospitalTripId);
        }

        public int SaveDiagnostic(int hospitalTripId, DateTime dateTime, 
            IDictionary<string,string> criterionsAndGrades)
        {
            List<CriterionGrade> grades = new List<CriterionGrade>();

            /* Creating grade objects */
            foreach (KeyValuePair<string, string> pair in criterionsAndGrades)
            {
                var criterion = _repository.Grades.CriterionDefinitions.
                            GetCriterionDefinitionByDefinition(pair.Key);

                CriterionGrade grade = new CriterionGrade()
                {
                    Grade = pair.Value,
                    CriterionDefinitionId = criterion.Id
                };

                grades.Add(grade);
            }

            /* Creating a diagnostic object */
            Diagnostic diagnostic = new Diagnostic()
            {
                Date = dateTime,
                HospitalTripId = hospitalTripId,
                CriterionGrades = grades
            };

            return _repository.SaveDiagnostic(diagnostic);
        }

        public void DeleteDiagnostic(int diagnosticId)
        {
            var diagnostic = GetDiagnostic(diagnosticId);
            _repository.DeleteDiagnosticCascade(diagnostic);
        }

        public int GetTotalSum(int diagnosticId)
        {
            return GetNumberGrades(diagnosticId).Sum();
        }

        public int GetPartialSum(int diagnosticId, int grade)
        {
            return GetNumberGrades(diagnosticId).Where(x => x == grade).Sum();
        }

        public IEnumerable<string> GetCriterions()
        {
            var criterionDefinitions = _repository.Grades.CriterionDefinitions.
                                                   GerCriterionDefinitions();

            return criterionDefinitions.Select(x => x.Definition);
        }

        private IEnumerable<int> GetNumberGrades(int diagnosticId)
        {
            /* Getting all diagnostic grades as objects */
            var grades = GetDiagnostic(diagnosticId).CriterionGrades;

            /* Getting grades that have point grade type as objects */
            var pointGrades = grades.Where(
                x => _repository.Grades.CriterionDefinitions.
                                          IsPointGradeTypeCriterion(
                                              x.CriterionDefinitionId));

            /* Getting grades as numbers */
            return pointGrades.Select(x => Convert.ToInt32(x.Grade));
        }
    }
}