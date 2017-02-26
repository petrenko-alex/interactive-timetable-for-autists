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

//        public int GetTotalSum(int diagnosticId)
//        {
//            var grades = GetDiagnostic(diagnosticId).CriterionGrades;
//
//            grades.Where(x => _repository.Grades.CriterionDefinitions.
//                                          IsPointGradeTypeCriterion(
//                                              x.CriterionDefinitionId));
//        }

        //public int GetPartialSum(int grade) {}

        public IEnumerable<string> GetCriterions()
        {
            var criterionDefinitions = _repository.Grades.CriterionDefinitions.
                                                   GerCriterionDefinitions();

            return criterionDefinitions.Select(x => x.Definition);
        }
    }
}