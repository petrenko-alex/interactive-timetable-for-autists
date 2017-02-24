using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class CriterionGradeType : BusinessEntityBase
    {
        [MaxLength(255), NotNull]
        public string TypeName { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CriterionDefinition> CriterionDefinitions { get; set; } 
            = new List<CriterionDefinition>();

        public override bool Equals(object obj)
        {
            /* If obj is null return false */
            if (obj == null)
            {
                return false;
            }

            /* If obj can not be cast to class type return false */
            CriterionGradeType type = obj as CriterionGradeType;
            if ((System.Object)type == null)
            {
                return false;
            }

            return TypeName.Equals(type.TypeName);

        }
    }
}
