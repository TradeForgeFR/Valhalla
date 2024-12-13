using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Valhalla.ViewModels.Docks;
using Valhalla.ViewModels.Documents;
 
namespace Valhalla.ViewModels;

public class ValhallaFactory : Factory
{
    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;
    
    public override IDocumentDock CreateDocumentDock() => new ChartsDocumentDock();

    public override IRootDock CreateLayout()
    {
        var untitledChartViewModel = new ChartViewModel()
        {
            Title = "Chart - Untitled"
        };

        var documentDock = new ChartsDocumentDock()
        {
            Id = "Charts",
            Title = "Charts",
            IsCollapsable = false,
            Proportion = double.NaN,
            ActiveDockable = untitledChartViewModel,
            VisibleDockables = CreateList<IDockable>
            (
                untitledChartViewModel
            ),
            CanCreateDocument = true
        };
        

        var windowLayout = CreateRootDock();
        windowLayout.Title = "Default";
        var windowLayoutContent = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>
            (
                documentDock
            )
        };
        windowLayout.IsCollapsable = false;
        windowLayout.VisibleDockables = CreateList<IDockable>(windowLayoutContent);
        windowLayout.ActiveDockable = windowLayoutContent;

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.VisibleDockables = CreateList<IDockable>(windowLayout);
        rootDock.ActiveDockable = windowLayout;
        rootDock.DefaultDockable = windowLayout;

        _documentDock = documentDock;
        _rootDock = rootDock;

        return rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            ["Root"] = () => _rootDock,
            ["Charts"] = () => _documentDock,
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }
}