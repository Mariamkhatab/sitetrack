# Site Issue & Snagging Tracker — Complete WPF Application

## Quick Start

### Step 1 — Create the project in Visual Studio
1. Open Visual Studio 2022
2. New Project → "WPF Application" (C#, Windows, Desktop) — NOT .NET Framework
3. Name: SnaggingTracker | Framework: .NET 8.0

### Step 2 — Install NuGet packages (Package Manager Console)
```
Install-Package CommunityToolkit.Mvvm
Install-Package ClosedXML
Install-Package Newtonsoft.Json
Install-Package Microsoft.Xaml.Behaviors.Wpf
```

### Step 3 — Create folders in Solution Explorer
Converters/ | Helpers/ | Models/ | Services/ | Themes/ | ViewModels/ | Views/

### Step 4 — Copy all files into their folders (Add → Existing Item)
- Helpers/       → Enums.cs
- Models/        → SnagIssue.cs
- Services/      → IDataService.cs, IExcelExportService.cs, JsonDataService.cs, ExcelExportService.cs
- ViewModels/    → MainViewModel.cs
- Converters/    → PriorityToColorConverter.cs, StatusToColorConverter.cs, MiscConverters.cs
- Themes/        → AppTheme.xaml
- Views/         → MainView.xaml, MainView.xaml.cs
- Root/          → App.xaml (Build Action = ApplicationDefinition), App.xaml.cs, SnaggingTracker.csproj

### Step 5 — Press F5

---

## Features
- Log / Edit / Delete snagging issues (Title, Description, Location, Priority, Status, Responsible Party, Dates, Remarks)
- Auto-reference numbers: SNAG-001, SNAG-002...
- Live search + filter by Priority and Status
- Stats bar: Total / Open / In Progress / Closed / Closure Rate %
- Excel export: Full 3-sheet report | Snag List only | Contractor Performance
- JSON persistence to %AppData%\SnaggingTracker\snags.json

## Architecture
- Strict MVVM: zero logic in code-behind
- CommunityToolkit [ObservableProperty] + [RelayCommand]
- Interfaces: IDataService, IExcelExportService
- All 5 phases complete
