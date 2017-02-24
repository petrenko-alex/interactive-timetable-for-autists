using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using NUnit.Framework;
using SQLite;
using InteractiveTimetable.DataAccessLayer;

namespace InteractiveTimetable.Tests.Repositories
{
    [TestFixture]
    class CriterionGradeTypeRepositoryTests
    {
        private SQLiteConnection _connection;
        private CriterionGradeTypeRepository _gradeTypeRepository;

        [SetUp]
        public void InitializationBeforeTests()
        {
            /* Creating connection */
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);

            /* Initialize repository */
            _gradeTypeRepository 
                = new CriterionGradeTypeRepository(_connection);
        }

        [TearDown]
        public void ShutDown()
        {
            _connection.Dispose();
            File.Delete("TestsDatabase.db3");
        }

        [Test]
        public void GetCriterionGradeTypes()
        {
            // arrange 

            // act
            var gradeTypes =
                    _gradeTypeRepository.GetCriterionGradeTypes().ToList();
            var tickGrade = gradeTypes.Where(x => x.TypeName == "TICK_GRADE");
            var pointGrade = gradeTypes.Where(x => x.TypeName == "POINT_GRADE");

            // assert
            Assert.AreEqual(2, gradeTypes.Count());
            Assert.NotNull(tickGrade);
            Assert.NotNull(pointGrade);
        }

        [Test]
        public void GetPointCriterionGradeType()
        {
            // arrange
            var gradeTypes =
                    _gradeTypeRepository.GetCriterionGradeTypes().ToList();
            var pointGrade = gradeTypes.First(x => x.TypeName == "POINT_GRADE");

            // act
            var gradeType =
                    _gradeTypeRepository.GetCriterionGradeTypeByNumber(10);

            // assert
            Assert.AreEqual(pointGrade, gradeType);
        }

        [Test]
        public void GetTickCriterionGradeType()
        {
            // arrange
            var gradeTypes =
                    _gradeTypeRepository.GetCriterionGradeTypes().ToList();
            var pointGrade = gradeTypes.First(x => x.TypeName == "TICK_GRADE");

            // act
            var gradeType =
                    _gradeTypeRepository.GetCriterionGradeTypeByNumber(18);

            // assert
            Assert.AreEqual(pointGrade, gradeType);
        }

        [Test]
        public void GetNotExistingGradeType()
        {
            // arrange
            string exceptionMessage = "Not a valid number.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _gradeTypeRepository.GetCriterionGradeTypeByNumber(19);
            });
            Assert.AreEqual(exceptionMessage, exception.Message);
        }

        [Test]
        public void DeletePointGradeType()
        {
            // arrange 
            var type = _gradeTypeRepository.GetPointCriterionGradeType();

            // act
            _gradeTypeRepository.DeleteCriterionGradeType(type.Id);
            var deletedType = _gradeTypeRepository.GetPointCriterionGradeType();

            // assert
            Assert.AreEqual(null, deletedType);
        }

        [Test]
        public void DeleteTickGradeType()
        {
            // arrange 
            var type = _gradeTypeRepository.GetTickCriterionGradeType();

            // act
            _gradeTypeRepository.DeleteCriterionGradeType(type.Id);
            var deletedType = _gradeTypeRepository.GetTickCriterionGradeType();

            // assert
            Assert.AreEqual(null, deletedType);
        }

        [Test]
        public void DeleteWithDefinitions()
        {
            // arrange
            var _definitionRepository 
                = new CriterionDefinitionRepository(_connection);

            var type = _gradeTypeRepository.GetTickCriterionGradeType();
            var definition = type.CriterionDefinitions.First();

            var check = _gradeTypeRepository.GetCriterionGradeTypes();
            // act
            _gradeTypeRepository.DeleteCriterionGradeTypeCascade(type);
            var deletedType = _gradeTypeRepository.GetCriterionGradeType(type.Id);
            var deletedDefinition =
                    _definitionRepository.GetCriterionDefinition(definition.Id);

            // assert
            Assert.AreEqual(null, deletedType);
            Assert.AreEqual(null, deletedDefinition);
        }
    }
}