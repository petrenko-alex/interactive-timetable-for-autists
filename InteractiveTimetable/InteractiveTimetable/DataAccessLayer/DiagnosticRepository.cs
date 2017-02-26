using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class DiagnosticRepository : BaseRepository
    {
        public CriterionGradeRepository Grades { get; }

        public DiagnosticRepository(SQLiteConnection connection)
            : base(connection)
        {
            Grades = new CriterionGradeRepository(connection);
        }

        public Diagnostic GetDiagnostic(int diagnosticId)
        {
            return _database.GetItemCascade<Diagnostic>(diagnosticId);
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return _database.GetItemsCascade<Diagnostic>();
        }

        public IEnumerable<Diagnostic> GetTripDiagnostics(int hospitalTripId)
        {
            var allDiagnostics = GetDiagnostics();

            /* 
             * Getting all diagnostics that was held during trip 
             * and ordered by date 
             */
            var tripDiagnostics = allDiagnostics.
                    Where(d => d.HospitalTripId == hospitalTripId).
                    OrderBy(d => d.Date);

            return tripDiagnostics;
        }

        public int SaveDiagnostic(Diagnostic diagnostic)
        {
            return _database.SaveItemCascade(diagnostic);
        }

        public int DeleteDiagnostic(int diagnosticId)
        {
            return _database.DeleteItem<Diagnostic>(diagnosticId);
        }

        public void DeleteDiagnosticCascade(Diagnostic diagnostic)
        {
            _database.DeleteItemCascade(diagnostic);
        }
    }
}