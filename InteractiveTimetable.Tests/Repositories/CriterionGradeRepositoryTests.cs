using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using NUnit.Framework;
using SQLite;

namespace InteractiveTimetable.Tests.Repositories
{
    [TestFixture]
    class CriterionGradeRepositoryTests
    {
        private SQLiteConnection _connection;
        private CriterionGradeRepository _repository;

        [SetUp]
        public void InitializationBeforeTests()
        {
            /* Creating connection */
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);

            /* Initialize repository */
            _repository = new CriterionGradeRepository(_connection);
        }

        [TearDown]
        public void ShutDown()
        {
            _connection.Dispose();
            File.Delete("TestsDatabase.db3");
        }

        [Test]
        public void SavePointGrade()
        {
            // arrange
            CriterionGrade grade = new CriterionGrade()
            {
                CriterionDefinitionId = 3,
                Grade = "2"
            };

            // act
            var gradeId = _repository.SaveCriterionGrade(grade);
            var addedGrade = _repository.GetCriterionGrade(gradeId);

            // assert
            Assert.AreEqual(grade, addedGrade);
        }

        [Test]
        public void SaveTickGrade()
        {
            // arrange
            CriterionGrade grade = new CriterionGrade()
            {
                CriterionDefinitionId = 18,
                Grade = "1001"
            };

            // act
            var gradeId = _repository.SaveCriterionGrade(grade);
            var addedGrade = _repository.GetCriterionGrade(gradeId);

            // assert
            Assert.AreEqual(grade, addedGrade);
        }

        [Test]
        public void SaveIncorrectPointGrade()
        {
            // arrange
            CriterionGrade grade1 = new CriterionGrade()
            {
                CriterionDefinitionId = 2,
                Grade = "0"
            };

            CriterionGrade grade2 = new CriterionGrade()
            {
                CriterionDefinitionId = 2,
                Grade = "5"
            };
            string exceptionMessage = "Not valid grade.";

            // act/assert
            var exception1 = Assert.Throws<ArgumentException>(delegate
            {
                _repository.SaveCriterionGrade(grade1);
            });

            var exception2 = Assert.Throws<ArgumentException>(delegate
            {
                _repository.SaveCriterionGrade(grade2);
            });

            Assert.AreEqual(exceptionMessage, exception1.Message);
            Assert.AreEqual(exceptionMessage, exception2.Message);
        }

        [Test]
        public void SaveIncorrectTickGrade()
        {
            // arrange
            CriterionGrade grade = new CriterionGrade()
            {
                CriterionDefinitionId = 18,
                Grade = "0"
            };
            string exceptionMessage = "Not valid grade.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _repository.SaveCriterionGrade(grade);
            });

            Assert.AreEqual(exceptionMessage, exception.Message);
        }

        [Test]
        public void SaveIncorrectGradeType()
        {
            // arrange
            CriterionGrade grade = new CriterionGrade()
            {
                CriterionDefinitionId = 23,
                Grade = "2"
            };
            string exceptionMessage = "Not valid grade type.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _repository.SaveCriterionGrade(grade);
            });

            Assert.AreEqual(exceptionMessage, exception.Message);
        }

        [Test]
        public void GetAllGrades()
        {
            // arrange
            CriterionGrade grade1 = new CriterionGrade()
            {
                CriterionDefinitionId = 2,
                Grade = "3"
            };

            CriterionGrade grade2 = new CriterionGrade()
            {
                CriterionDefinitionId = 2,
                Grade = "4"
            };
            _repository.SaveCriterionGrade(grade1);
            _repository.SaveCriterionGrade(grade2);

            // act
            var grades = _repository.GetCriterionGrades();

            // assert
            Assert.AreEqual(2, grades.Count());
        }

        [Test]
        public void GetAllDiagnosticGrades()
        {
            // arrange
            CriterionGrade grade1 = new CriterionGrade()
            {
                CriterionDefinitionId = 2,
                DiagnosticId = 1,
                Grade = "3"
            };

            CriterionGrade grade2 = new CriterionGrade()
            {
                CriterionDefinitionId = 2,
                DiagnosticId =  1,
                Grade = "4"
            };

            CriterionGrade grade3 = new CriterionGrade()
            {
                CriterionDefinitionId = 2,
                DiagnosticId = 2,
                Grade = "4"
            };
            _repository.SaveCriterionGrade(grade1);
            _repository.SaveCriterionGrade(grade2);
            _repository.SaveCriterionGrade(grade3);

            // act
            var diagnosticGrades = _repository.GetDiagnosticCriterionGrades(1);

            // assert
            Assert.AreEqual(2, diagnosticGrades.Count());
        }

        [Test]
        public void DeleteCriterionGrade()
        {
            // arrange
            CriterionGrade grade = new CriterionGrade()
            {
                CriterionDefinitionId = 2,
                Grade = "2",
            };
            var gradeId = _repository.SaveCriterionGrade(grade);

            // act
            _repository.DeleteCriterionGrade(gradeId);
            var deletedGrade = _repository.GetCriterionGrade(gradeId);

            // assert
            Assert.AreEqual(null, deletedGrade);
        }

        [Test]
        public void DeleteNotExistingGrade()
        {
            // arrange

            // act
            _repository.DeleteCriterionGrade(21);
            var deletedGrade = _repository.GetCriterionGrade(21);

            // assert
            Assert.AreEqual(null, deletedGrade);
            Assert.Pass("No exceptions");
        }
    }

}
