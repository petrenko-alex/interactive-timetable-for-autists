using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class CriterionDefinition : BusinessEntityBase
    {
        [NotNull]
        public int Number { get; set; }

        [MaxLength(255), NotNull]
        public string Definition { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CriterionGrade> CriterionGrades { get; set; } 
            = new List<CriterionGrade>();

        [ForeignKey(typeof(CriterionGradeType))]
        public int CriterionGradeTypeId { get; set; }

        public override bool Equals(object obj)
        {
            CriterionDefinition definition = obj as CriterionDefinition;
            if (definition == null)
            {
                return false;
            }

            return Number.Equals(definition.Number) &&
                   Definition.Equals(definition.Definition) &&
                   CriterionGradeTypeId.Equals(definition.CriterionGradeTypeId);
        }

        public bool Equals(CriterionDefinition obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Number.Equals(obj.Number) &&
                   Definition.Equals(obj.Definition) &&
                   CriterionGradeTypeId.Equals(obj.CriterionGradeTypeId);
        }

        public override string ToString()
        {
            return Definition;
        }
    }
}