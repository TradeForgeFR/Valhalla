using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using Valhalla.ViewModels.Documents;

namespace Valhalla.ViewModels.Docks;

public class ChartsDocumentDock : DocumentDock
{
    public ChartsDocumentDock()
    {
        CreateDocument = new RelayCommand(CreateNewDocument);
    }

    private void CreateNewDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        var document = new ChartViewModel()
        {
            Title = "Chart - Untitled"
        };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}