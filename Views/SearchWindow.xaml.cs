using System;
using System.Windows;
using System.Windows.Input;
using LiquidGlassShell.Services;

namespace LiquidGlassShell.Views
{
    public partial class SearchWindow : Window
    {
        private readonly SearchService _searchService;

        public SearchWindow(SearchService searchService)
            : base()
        {
            _searchService = searchService;

            // Check if SearchTextBox exists before subscribing events
            var searchTextBox = this.FindName("SearchTextBox") as System.Windows.Controls.TextBox;
            var resultsListBox = this.FindName("ResultsListBox") as System.Windows.Controls.ListBox;

            if (searchTextBox != null)
            {
                searchTextBox.KeyDown += SearchTextBox_KeyDown;
                searchTextBox.Focus();
            }

            if (resultsListBox != null)
            {
                resultsListBox.MouseDoubleClick += ResultsListBox_MouseDoubleClick;
            }

            this.KeyDown += SearchWindow_KeyDown;
        }

        private void SearchWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Hide();
            }
        }

        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var resultsListBox = this.FindName("ResultsListBox") as System.Windows.Controls.ListBox;
            var searchTextBox = this.FindName("SearchTextBox") as System.Windows.Controls.TextBox;
            if (e.Key == Key.Enter && resultsListBox != null && resultsListBox.SelectedItem != null)
            {
                // Abrir resultado seleccionado
                var result = resultsListBox.SelectedItem as SearchResult;
                if (result != null)
                {
                    OpenResult(result);
                }
            }
            else
            {
                // Buscar mientras escribe
                if (searchTextBox != null && resultsListBox != null)
                {
                    var query = searchTextBox.Text;
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        var results = await _searchService.SearchAsync(query);
                        resultsListBox.ItemsSource = results;
                    if (results.Count > 0)
                    {
                        resultsListBox.SelectedIndex = 0;
                    }
                }
            }
        }}

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as System.Windows.Controls.ListBox;
            if (listBox != null)
            {
                var result = listBox.SelectedItem as SearchResult;
                if (result != null)
                {
                    OpenResult(result);
                }
            }
        }

        private void OpenResult(SearchResult result)
        {
            // Implementar lógica para abrir el resultado
            this.Hide();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var searchTextBox = this.FindName("SearchTextBox") as System.Windows.Controls.TextBox;
            var resultsListBox = this.FindName("ResultsListBox") as System.Windows.Controls.ListBox;
            
            if (searchTextBox != null)
            {
                searchTextBox.Text = string.Empty;
                searchTextBox.Focus();
            }
            
            if (resultsListBox != null)
            {
                resultsListBox.ItemsSource = null;
            }
        }
    }
}

