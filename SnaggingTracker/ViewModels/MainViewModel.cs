using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SnaggingTracker.Helpers;
using SnaggingTracker.Models;
using SnaggingTracker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SnaggingTracker.ViewModels
{
    /// <summary>
    /// Primary ViewModel for the main application window.
    /// Owns the full issue list, filtering, CRUD, and export commands.
    /// All UI state and business logic lives here — zero logic in code-behind.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly IExcelExportService _excelService;

        // ── Backing list (all issues unfiltered) ─────────────────────────
        private List<SnagIssue> _allIssues = new();

        // ── Observable Collections ────────────────────────────────────────
        [ObservableProperty]
        private ObservableCollection<SnagIssue> _filteredIssues = new();

        // ── Selected item ─────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedIssue))]
        [NotifyCanExecuteChangedFor(nameof(DeleteIssueCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveEditCommand))]
        private SnagIssue? _selectedIssue;

        public bool HasSelectedIssue => SelectedIssue != null;

        // ── Form fields (Add / Edit panel) ────────────────────────────────
        [ObservableProperty] private string _formTitle = string.Empty;
        [ObservableProperty] private string _formDescription = string.Empty;
        [ObservableProperty] private string _formLocation = string.Empty;
        [ObservableProperty] private string _formResponsibleParty = string.Empty;
        [ObservableProperty] private string _formRemarks = string.Empty;
        [ObservableProperty] private IssuePriority _formPriority = IssuePriority.Medium;
        [ObservableProperty] private IssueStatus _formStatus = IssueStatus.Open;
        [ObservableProperty] private DateTime _formDateLogged = DateTime.Today;
        [ObservableProperty] private DateTime? _formDueDate;

        // ── Filter state ──────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFiltered))]
        private string _filterText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFiltered))]
        private IssuePriority? _filterPriority;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFiltered))]
        private IssueStatus? _filterStatus;

        public bool IsFiltered => !string.IsNullOrWhiteSpace(FilterText) || FilterPriority.HasValue || FilterStatus.HasValue;

        // ── UI State ──────────────────────────────────────────────────────
        [ObservableProperty] private bool _isAddPanelOpen;
        [ObservableProperty] private bool _isEditPanelOpen;
        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private string _statusMessage = "Ready";
        [ObservableProperty] private bool _isStatusError;

        // ── Statistics (header bar) ───────────────────────────────────────
        [ObservableProperty] private int _totalCount;
        [ObservableProperty] private int _openCount;
        [ObservableProperty] private int _inProgressCount;
        [ObservableProperty] private int _closedCount;
        [ObservableProperty] private double _closureRate;

        // ── Drop-down sources ─────────────────────────────────────────────
        public IReadOnlyList<IssuePriority> PriorityValues   { get; } = Enum.GetValues<IssuePriority>().ToList();
        public IReadOnlyList<IssueStatus>   StatusValues     { get; } = Enum.GetValues<IssueStatus>().ToList();
        public IReadOnlyList<IssuePriority?> FilterPriorities { get; } = new List<IssuePriority?> { null, IssuePriority.High, IssuePriority.Medium, IssuePriority.Low };
        public IReadOnlyList<IssueStatus?>   FilterStatuses   { get; } = new List<IssueStatus?>   { null, IssueStatus.Open, IssueStatus.InProgress, IssueStatus.Closed };

        // ─────────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────────
        public MainViewModel(IDataService dataService, IExcelExportService excelService)
        {
            _dataService  = dataService;
            _excelService = excelService;
        }

        // ─────────────────────────────────────────────────────────────────
        // COMMANDS — Load
        // ─────────────────────────────────────────────────────────────────
        [RelayCommand]
        private async Task LoadIssuesAsync()
        {
            IsBusy = true;
            StatusMessage = "Loading issues…";
            try
            {
                _allIssues = await _dataService.LoadIssuesAsync();
                ApplyFilters();
                UpdateStats();
                StatusMessage = $"Loaded {_allIssues.Count} issue(s).";
            }
            catch (Exception ex)
            {
                SetError($"Failed to load: {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        // ─────────────────────────────────────────────────────────────────
        // COMMANDS — Add
        // ─────────────────────────────────────────────────────────────────
        [RelayCommand]
        private void OpenAddPanel()
        {
            ClearForm();
            IsEditPanelOpen = false;
            IsAddPanelOpen  = true;
        }

        [RelayCommand]
        private void CloseAddPanel() => IsAddPanelOpen = false;

        [RelayCommand]
        private async Task AddIssueAsync()
        {
            if (string.IsNullOrWhiteSpace(FormTitle))
            {
                SetError("Title is required.");
                return;
            }

            var issue = new SnagIssue
            {
                Id               = Guid.NewGuid(),
                ReferenceNumber  = _dataService.GenerateReferenceNumber(_allIssues),
                Title            = FormTitle.Trim(),
                Description      = FormDescription.Trim(),
                Location         = FormLocation.Trim(),
                ResponsibleParty = FormResponsibleParty.Trim(),
                Remarks          = FormRemarks.Trim(),
                Priority         = FormPriority,
                Status           = FormStatus,
                DateLogged       = FormDateLogged,
                DueDate          = FormDueDate,
            };

            _allIssues.Add(issue);
            await SaveAsync();
            ApplyFilters();
            UpdateStats();
            IsAddPanelOpen = false;
            StatusMessage = $"Added {issue.ReferenceNumber}: {issue.Title}";
        }

        // ─────────────────────────────────────────────────────────────────
        // COMMANDS — Edit
        // ─────────────────────────────────────────────────────────────────
        [RelayCommand]
        private void OpenEditPanel()
        {
            if (SelectedIssue == null) return;
            PopulateFormFrom(SelectedIssue);
            IsAddPanelOpen  = false;
            IsEditPanelOpen = true;
        }

        [RelayCommand]
        private void CloseEditPanel() => IsEditPanelOpen = false;

        [RelayCommand(CanExecute = nameof(HasSelectedIssue))]
        private async Task SaveEditAsync()
        {
            if (SelectedIssue == null) return;
            if (string.IsNullOrWhiteSpace(FormTitle))
            {
                SetError("Title is required.");
                return;
            }

            SelectedIssue.Title            = FormTitle.Trim();
            SelectedIssue.Description      = FormDescription.Trim();
            SelectedIssue.Location         = FormLocation.Trim();
            SelectedIssue.ResponsibleParty = FormResponsibleParty.Trim();
            SelectedIssue.Remarks          = FormRemarks.Trim();
            SelectedIssue.Priority         = FormPriority;
            SelectedIssue.DueDate          = FormDueDate;

            // If status just changed to Closed, stamp the resolved date
            if (FormStatus == IssueStatus.Closed && SelectedIssue.Status != IssueStatus.Closed)
                SelectedIssue.DateResolved = DateTime.Today;
            else if (FormStatus != IssueStatus.Closed)
                SelectedIssue.DateResolved = null;

            SelectedIssue.Status = FormStatus;

            await SaveAsync();
            ApplyFilters();
            UpdateStats();
            IsEditPanelOpen = false;
            StatusMessage = $"Updated {SelectedIssue.ReferenceNumber}.";
        }

        // ─────────────────────────────────────────────────────────────────
        // COMMANDS — Delete
        // ─────────────────────────────────────────────────────────────────
        [RelayCommand(CanExecute = nameof(HasSelectedIssue))]
        private async Task DeleteIssueAsync()
        {
            if (SelectedIssue == null) return;

            var result = MessageBox.Show(
                $"Delete issue {SelectedIssue.ReferenceNumber}: \"{SelectedIssue.Title}\"?\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            string refNum = SelectedIssue.ReferenceNumber;
            _allIssues.Remove(SelectedIssue);
            SelectedIssue = null;
            await SaveAsync();
            ApplyFilters();
            UpdateStats();
            StatusMessage = $"Deleted {refNum}.";
        }

        // ─────────────────────────────────────────────────────────────────
        // COMMANDS — Filter
        // ─────────────────────────────────────────────────────────────────
        [RelayCommand]
        private void ApplyFilter() => ApplyFilters();

        [RelayCommand]
        private void ClearFilters()
        {
            FilterText     = string.Empty;
            FilterPriority = null;
            FilterStatus   = null;
            ApplyFilters();
        }

        // ─────────────────────────────────────────────────────────────────
        // COMMANDS — Export
        // ─────────────────────────────────────────────────────────────────
        [RelayCommand]
        private async Task ExportFullReportAsync()  => await RunExport("Full Report (3 Sheets)",
            fp => _excelService.ExportFullReportAsync(_allIssues, fp));

        [RelayCommand]
        private async Task ExportSnagListAsync()    => await RunExport("Snag List",
            fp => _excelService.ExportSnagListAsync(_allIssues, fp));

        [RelayCommand]
        private async Task ExportContractorAsync()  => await RunExport("Contractor Performance",
            fp => _excelService.ExportContractorReportAsync(_allIssues, fp));

        private async Task RunExport(string reportName, Func<string, Task<bool>> exportAction)
        {
            if (_allIssues.Count == 0)
            {
                MessageBox.Show("No issues to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title            = $"Export — {reportName}",
                Filter           = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName         = $"SnaggingReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                DefaultExt       = ".xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dlg.ShowDialog() != true) return;

            IsBusy = true;
            StatusMessage = $"Exporting {reportName}…";
            try
            {
                bool ok = await exportAction(dlg.FileName);
                StatusMessage = ok
                    ? $"Exported '{reportName}' to {System.IO.Path.GetFileName(dlg.FileName)}"
                    : "Export failed — file may be open in Excel.";
                IsStatusError = !ok;

                if (ok && MessageBox.Show("Export complete. Open file?", "Export", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex) { SetError($"Export error: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // ─────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────
        private async Task SaveAsync()
        {
            try { await _dataService.SaveIssuesAsync(_allIssues); }
            catch (Exception ex) { SetError($"Save error: {ex.Message}"); }
        }

        private void ApplyFilters()
        {
            var result = _allIssues.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                string q = FilterText.Trim().ToLowerInvariant();
                result = result.Where(i =>
                    i.Title.ToLowerInvariant().Contains(q)           ||
                    i.ReferenceNumber.ToLowerInvariant().Contains(q) ||
                    i.Location.ToLowerInvariant().Contains(q)        ||
                    i.ResponsibleParty.ToLowerInvariant().Contains(q));
            }

            if (FilterPriority.HasValue)
                result = result.Where(i => i.Priority == FilterPriority.Value);

            if (FilterStatus.HasValue)
                result = result.Where(i => i.Status == FilterStatus.Value);

            FilteredIssues = new ObservableCollection<SnagIssue>(result.OrderByDescending(i => i.DateLogged));
        }

        private void UpdateStats()
        {
            TotalCount      = _allIssues.Count;
            OpenCount       = _allIssues.Count(i => i.Status == IssueStatus.Open);
            InProgressCount = _allIssues.Count(i => i.Status == IssueStatus.InProgress);
            ClosedCount     = _allIssues.Count(i => i.Status == IssueStatus.Closed);
            ClosureRate     = TotalCount > 0 ? Math.Round((double)ClosedCount / TotalCount * 100, 1) : 0;
        }

        private void ClearForm()
        {
            FormTitle            = string.Empty;
            FormDescription      = string.Empty;
            FormLocation         = string.Empty;
            FormResponsibleParty = string.Empty;
            FormRemarks          = string.Empty;
            FormPriority         = IssuePriority.Medium;
            FormStatus           = IssueStatus.Open;
            FormDateLogged       = DateTime.Today;
            FormDueDate          = null;
            IsStatusError        = false;
        }

        private void PopulateFormFrom(SnagIssue issue)
        {
            FormTitle            = issue.Title;
            FormDescription      = issue.Description;
            FormLocation         = issue.Location;
            FormResponsibleParty = issue.ResponsibleParty;
            FormRemarks          = issue.Remarks;
            FormPriority         = issue.Priority;
            FormStatus           = issue.Status;
            FormDateLogged       = issue.DateLogged;
            FormDueDate          = issue.DueDate;
            IsStatusError        = false;
        }

        private void SetError(string message)
        {
            StatusMessage = message;
            IsStatusError = true;
        }

        // Notify filter-dependent computed properties when filter fields change
        partial void OnFilterTextChanged(string value)     => ApplyFilters();
        partial void OnFilterPriorityChanged(IssuePriority? value) => ApplyFilters();
        partial void OnFilterStatusChanged(IssueStatus? value)   => ApplyFilters();
    }
}
