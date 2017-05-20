using System.Threading.Tasks;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public interface IDiagnosticDialogListener
    {
        Task OnNewDiagnosticAdded(int diagnosticId);
        Task OnDiagnosticEdited(int diagnosticId);
    }
}