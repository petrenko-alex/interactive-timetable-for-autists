using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class CriterionGrade : BusinessEntityBase
    {
        [MaxLength(255), NotNull]
        public string Grade { get; set; }

        [NotNull]
        public bool IsDeleted { get; set; } = false;

        [ForeignKey(typeof(Diagnostic))]
        public int DiagnosticId { get; set; }

        [ForeignKey(typeof(CriterionDefinition))]
        public int CriterionDefinitionId { get; set; }

        public override bool Equals(object obj)
        {
            CriterionGrade criterionGrade = obj as CriterionGrade;
            if (criterionGrade == null)
            {
                return false;
            }

            return Grade.Equals(criterionGrade.Grade) &&
                   CriterionDefinitionId.
                           Equals(criterionGrade.CriterionDefinitionId) &&
                   DiagnosticId.Equals(criterionGrade.DiagnosticId);
        }

        public bool Equals(CriterionGrade obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Grade.Equals(obj.Grade) &&
                   CriterionDefinitionId.Equals(obj.CriterionDefinitionId) &&
                   DiagnosticId.Equals(obj.DiagnosticId);
        }
    }
}