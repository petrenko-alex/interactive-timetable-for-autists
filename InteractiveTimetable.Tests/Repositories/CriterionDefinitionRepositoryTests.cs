using System;
using System.IO;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using NUnit.Framework;
using SQLite;

namespace InteractiveTimetable.Tests.Repositories
{
    [TestFixture]
    class CriterionDefinitionRepositoryTests
    {
        private SQLiteConnection _connection;
        private CriterionDefinitionRepository _repository;

        [SetUp]
        public void InitializationBeforeTests()
        {
            /* Creating connection */
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);

            /* Initialize repository */
            _repository = new CriterionDefinitionRepository(_connection);
        }

        [TearDown]
        public void ShutDown()
        {
            _connection.Dispose();
            File.Delete("TestsDatabase.db3");
        }

        [Test]
        public void GetCriterionDefinitions()
        {
            // arrange

            // act
            var criterions = _repository.GerCriterionDefinitions();

            // assert
            Assert.AreEqual(_repository.GetNumberOfCriterions(),
                            criterions.Count());
        }

        [Test]
        public void DeleteCriterionDefinition()
        {
            // arrange
            var criterionId =
                    _repository.GerCriterionDefinitions().ToList()[0].Id;

            // act
            _repository.DeleteCriterionDefinition(criterionId);
            var criterion = _repository.GetCriterionDefinition(criterionId);

            // assert
            Assert.AreEqual(null, criterion);
        }

        [Test]
        public void DeleteCriterionDefinitionWithGrades()
        {
            /* Create new database with another repository for this test */
            ShutDown();
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);
            var _repository = new CriterionGradeRepository(_connection);

            // arrange
            int criterionId = 3;
            CriterionGrade grade1 = new CriterionGrade()
            {
                CriterionDefinitionId = criterionId,
                Grade = "2"
            };

            CriterionGrade grade2 = new CriterionGrade()
            {
                CriterionDefinitionId = criterionId,
                Grade = "3"
            };
            var grade1Id = _repository.SaveCriterionGrade(grade1);
            var grade2Id = _repository.SaveCriterionGrade(grade2);

            // act 1
            var criterion = _repository.
                    CriterionDefinitions.
                    GetCriterionDefinition(criterionId);

            // assert 1
            Assert.AreEqual(2, criterion.CriterionGrades.Count);

            // act 2
            _repository.CriterionDefinitions.
                DeleteCriterionDefinitionCascade(criterion);

            var deletedCriterion = _repository.
                    CriterionDefinitions.
                    GetCriterionDefinition(criterionId);
            var deletedGrade1 = _repository.GetCriterionGrade(grade1Id);
            var deletedGrade2 = _repository.GetCriterionGrade(grade2Id);

            // assert 2
            Assert.AreEqual(null, deletedCriterion);
            Assert.AreEqual(null, deletedGrade1);
            Assert.AreEqual(null, deletedGrade2);
        }

        [Test]
        public void GetCriterionDefinitionByNumber()
        {
            // arrange
            int number = 5;

            // act
            var criterion = _repository.GetCriterionDefinitionByNumber(number);

            // assert
            Assert.AreEqual(number, criterion.Number);
        }

        [Test]
        public void GetCriterionDefinitionByNotValidNumber()
        {
            // arrange
            int number = -2;
            var exceptionMessage = Resources.Validation.
                                             CriterionDefinitionValidationStrings.
                                             NotValidNumber;

            // act/assert
            var exception1 = Assert.Throws<ArgumentException>(delegate
            {
                _repository.GetCriterionDefinitionByNumber(number);
            });

            number = 19;

            var exception2 = Assert.Throws<ArgumentException>(delegate
            {
                _repository.GetCriterionDefinitionByNumber(number);
            });

            Assert.AreEqual(exceptionMessage, exception1.Message);
            Assert.AreEqual(exceptionMessage, exception2.Message);
        }

        [Test]
        public void GetCriterionDefinitionByDefinition()
        {
            // arrange
            var definition = Resources.Repositories.CriterionDefinitionStrings.
                    Criterion13;
            var expectedCriterion =
                    _repository.GetCriterionDefinitionByNumber(13);

            // act
            var criterion =
                    _repository.GetCriterionDefinitionByDefinition(definition);

            // assert
            Assert.AreEqual(expectedCriterion, criterion);
        }

        [Test]
        public void GetCriterionDefinitionByNotValidDefinition()
        {
            // arrange
            var definition = "not valid";
            var exceptionString = Resources.Validation.
                                            CriterionDefinitionValidationStrings.
                                            NotValidDefinition;

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _repository.GetCriterionDefinitionByDefinition(definition);
            });

            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void IsPointGradeTypeCriterion()
        {
            // arrange
            var pointCriterion = _repository.GetCriterionDefinitionByNumber(3);
            var tickCriterion = _repository.GetCriterionDefinitionByNumber(18);

            // act
            bool isPoint
                    = _repository.IsPointGradeTypeCriterion(pointCriterion.Id);
            bool isNotPoint
                    = _repository.IsPointGradeTypeCriterion(tickCriterion.Id);

            // assert
            Assert.AreEqual(true, isPoint);
            Assert.AreEqual(false, isNotPoint);
        }

        [Test]
        public void IsTickGradeTypeCriterion()
        {
            // arrange
            var pointCriterion = _repository.GetCriterionDefinitionByNumber(3);
            var tickCriterion = _repository.GetCriterionDefinitionByNumber(18);

            // act
            bool isTick
                    = _repository.IsTickGradeTypeCriterion(tickCriterion.Id);
            bool isNotTick
                    = _repository.IsTickGradeTypeCriterion(pointCriterion.Id);

            // assert
            Assert.AreEqual(true, isTick);
            Assert.AreEqual(false, isNotTick);
        }

        [Test]
        public void GetCriterionGradeType()
        {
            // arrange
            var pointCriterion = _repository.GetCriterionDefinitionByNumber(3);
            var tickCriterion = _repository.GetCriterionDefinitionByNumber(18);

            // act
            var pointGradeType
                    = _repository.GetCriterionGradeType(pointCriterion.Id);
            var tickGradeType
                    = _repository.GetCriterionGradeType(tickCriterion.Id);

            // assert
            Assert.AreEqual("POINT_GRADE", pointGradeType.TypeName);
            Assert.AreEqual("TICK_GRADE", tickGradeType.TypeName);
        }
    }
}