using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class DiagnosticRepository : BaseRepository
    {
        internal CriterionGradeRepository Grades { get; }

        internal DiagnosticRepository(SQLiteConnection connection)
            : base(connection)
        {
            Grades = new CriterionGradeRepository(connection);
        }

        internal Diagnostic GetDiagnostic(int diagnosticId)
        {
            return _database.GetItemCascade<Diagnostic>(diagnosticId);
        }

        internal IEnumerable<Diagnostic> GetDiagnostics()
        {
            return _database.GetItemsCascade<Diagnostic>();
        }

        internal IEnumerable<Diagnostic> GetTripDiagnostics(int hospitalTripId)
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

        internal int SaveDiagnostic(Diagnostic diagnostic)
        {
            return _database.SaveItemCascade(diagnostic);
        }

        internal int DeleteDiagnostic(int diagnosticId)
        {
            return _database.DeleteItem<Diagnostic>(diagnosticId);
        }

        internal void DeleteDiagnosticCascade(Diagnostic diagnostic)
        {
            _database.DeleteItemCascade(diagnostic);
        }
    }
}