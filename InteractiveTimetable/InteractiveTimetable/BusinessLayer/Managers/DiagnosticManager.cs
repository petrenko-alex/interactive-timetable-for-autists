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
        public int DiagnosticCount { get; private set; }

        private readonly DiagnosticRepository _repository;
        private readonly HospitalTripManager _hospitalTripManager;

        public DiagnosticManager(SQLiteConnection connection)
        {
            _repository = new DiagnosticRepository(connection);
            _hospitalTripManager = new HospitalTripManager(connection);
            DiagnosticCount = _repository.GetDiagnostics().Count();
        }

        public Diagnostic GetDiagnostic(int diagnosticId)
        {
            return _repository.GetDiagnostic(diagnosticId);
        }

        public IEnumerable<Diagnostic> GetDiagnostics(int hospitalTripId)
        {
            return _repository.GetTripDiagnostics(hospitalTripId);
        }

        public IDictionary<string, string> GetCriterionsAndGrades(
            int diagnosticId)
        {
            var grades = GetGrades(diagnosticId).ToList();
            return GetCriterionsAndGrades(diagnosticId, grades);
        }

        public int SaveDiagnostic(int hospitalTripId, DateTime dateTime, 
            IDictionary<string,string> criterionsAndGrades)
        {
            /* Data validation */
            Validate(hospitalTripId, dateTime, criterionsAndGrades);

            /* Creating grade objects */
            var grades = CreateGrades(criterionsAndGrades).ToList();

            /* Creating a diagnostic object */
            var diagnostic = new Diagnostic()
            {
                Date = dateTime,
                HospitalTripId = hospitalTripId,
                CriterionGrades = grades
            };

            var savedId = _repository.SaveDiagnostic(diagnostic);

            if (savedId > 0)
            {
                DiagnosticCount++;
            }

            return savedId;
        }

        public int UpdateDiagnostic(int diagnosticId, DateTime dateTime, 
            IDictionary<string, string> criterionsAndGrades)
        {
            var diagnostic = GetDiagnostic(diagnosticId);
            
            /* Data validation */
            Validate(diagnostic.HospitalTripId,
                     dateTime,
                     criterionsAndGrades);

            /* Updating diagnostic data */
            UpdateGrades(diagnostic, criterionsAndGrades);
            diagnostic.Date = dateTime;

            return _repository.SaveDiagnostic(diagnostic);

        }

        public void DeleteDiagnostic(int diagnosticId)
        {
            var diagnostic = GetDiagnostic(diagnosticId);
            if (diagnostic != null)
            {
                _repository.DeleteDiagnosticCascade(diagnostic);
                DiagnosticCount--;
            }
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

        public IEnumerable<string> GetGrades(int diagnosticId)
        {
            return GetGrades(GetDiagnostic(diagnosticId));
        }

        public int GetNumberOfCriterions()
        {
            return _repository.Grades.CriterionDefinitions.
                                GetNumberOfCriterions();
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

        private void Validate(int hospitalTripId, DateTime dateTime,
            IDictionary<string, string> criterionsAndGrades)
        {
            /* Checking that ... */

            /* ... criterions and grades are set */
            int amountOfCriterionsAndGrades = criterionsAndGrades.Count;
            if (criterionsAndGrades == null || amountOfCriterionsAndGrades == 0)
            {
                throw new ArgumentException(
                    Resources.
                    Validation.
                    DiagnosticManagerValidationStrings.
                    CriterionsAreNotPresent);
            }

            /* ... proper amount of criterions and grade is set */
            if (amountOfCriterionsAndGrades != GetNumberOfCriterions())
            {
                throw new ArgumentException(
                    Resources.
                    Validation.
                    DiagnosticManagerValidationStrings.
                    NotAllCriterionsPresent);
            }

            /* ... diagnostic date is not outside of hospital trip */
            var trip = _hospitalTripManager.
                GetHospitalTrip(hospitalTripId);

            if (dateTime < trip.StartDate ||
                dateTime > trip.FinishDate)
            {
                throw new ArgumentException(
                    Resources.
                    Validation.
                    DiagnosticManagerValidationStrings.
                    DiagnosticDateOutOfTrip);
            }
        }

        private IEnumerable<string> GetGrades(Diagnostic diagnostic)
        {
            return diagnostic.CriterionGrades.OrderBy(
                x => x.CriterionDefinitionId).Select(x => x.Grade);
        }

        private IDictionary<string, string> GetCriterionsAndGrades(
            int diagnosticId, List<string> grades)
        {
            IDictionary<string, string> criterionsAndGrades
                = new Dictionary<string, string>();

            var keys = GetCriterions().ToList();
            var amountOfKeys = keys.Count;

            for (int i = 0; i < amountOfKeys; ++i)
            {
                criterionsAndGrades.Add(
                    new KeyValuePair<string, string>(keys[i], grades[i]));
            }

            return criterionsAndGrades;
        }

        private IEnumerable<CriterionGrade> CreateGrades(
            IDictionary<string, string> criterionsAndGrades)
        {
            var grades = new List<CriterionGrade>();
            foreach (var pair in criterionsAndGrades)
            {
                var criterion = _repository.Grades.CriterionDefinitions.
                            GetCriterionDefinitionByDefinition(pair.Key);

                var grade = new CriterionGrade()
                {
                    Grade = pair.Value,
                    CriterionDefinitionId = criterion.Id
                };

                /* Validate grade before saving */
                _repository.Grades.Validate(grade);

                grades.Add(grade);
            }

            return grades;
        }

        private void UpdateGrades(Diagnostic diagnostic, 
            IDictionary<string, string> criterionsAndGrades)
        {
            // TODO: Add this to method comment
            /* 
             * !!! 
             *     Both criterionsAndGrades and diagnostic.CriterionGrades 
             *     must be ordered by CriterionDefinitionId(Number) and their
             *     sizes must be equal too.
             * !!!
             */
            var amountOfGrades = diagnostic.CriterionGrades.Count;
            var currentGrades = diagnostic.CriterionGrades.
                                           OrderBy(x => x.CriterionDefinitionId).
                                           ToList();

            for (int i = 0; i < amountOfGrades; ++i)
            {
                var criterionId = currentGrades[i].CriterionDefinitionId;
                var key = _repository.
                        Grades.
                        CriterionDefinitions.
                        GetCriterionDefinition(criterionId).ToString();

                currentGrades[i].Grade = criterionsAndGrades[key];
                _repository.Grades.SaveCriterionGrade(currentGrades[i]);
            }
        }

        public void InitializeForDebugging(UserManager userManager, HospitalTripManager tripManager)
        {
            /* Create diagnostics for each user */
            var randomizer = new Random();
            var users = userManager.GetUsers();

            foreach (var user in users)
            {
                /* Create diagnostics for each user trip */
                // TODO: var createOrNot = randomizer.Next(0, 2);
                var userTrips = user.HospitalTrips.ToList();

                foreach (var trip in userTrips)
                {
                    /* Create diagnostics */
                    // TODO: Change diagnostics amount range to [0;4)
                    var diagnosticsAmount = randomizer.Next(1, 4);
                    for (int i = 0; i < diagnosticsAmount; i++)
                    {
                        /* Create single diagnostic */
                        var criterion18 = Resources.Repositories.CriterionDefinitionStrings.
                                                    Criterion18;
                        var criterionAndGrades = new Dictionary<string, string>();
                        var keys = GetCriterions().ToList();

                        foreach (var key in keys)
                        {
                            var grade = randomizer.Next(1, 5).ToString();
                            if (key.Equals(criterion18))
                            {
                                grade = "1010";
                            }

                            criterionAndGrades.Add(key, grade);
                        }

                        int tripInDays = (trip.FinishDate - trip.StartDate).Days;
                        var daysToAddToStartDate = randomizer.Next(1, tripInDays);
                        SaveDiagnostic(
                            trip.Id,
                            trip.StartDate.AddDays(daysToAddToStartDate),
                            criterionAndGrades
                        );
                    }
                }
            }
        }
    }
}