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

        //public int SaveDiagnostic() {}

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