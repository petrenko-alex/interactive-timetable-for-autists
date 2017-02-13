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
    }
}
