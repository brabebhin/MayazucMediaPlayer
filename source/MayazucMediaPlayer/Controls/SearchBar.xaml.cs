using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class SearchBar : UserControl
    {
        private readonly object lockObject = new object();

        public SearchBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The collection view source bound to the list view. You may use this one, or your own
        /// </summary>
        public AdvancedCollectionView SearchView
        {
            get;
            set;
        }

        private void SearchBar_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SearchInternal();
        }

        /// <summary>
        /// Gets or sets a value representing whether the search should happen real time, or wait till button press
        /// </summary>
        public bool RealTime
        {
            get;
            set;
        } = true;

        public Func<object, string> Filter
        {
            get;
            set;
        }

        public static DependencyProperty SearchViewProperty = DependencyProperty.Register("SearchView", typeof(AdvancedCollectionView), typeof(SearchBar), new PropertyMetadata(new AdvancedCollectionView(), SearchViewPropertyChanged));
        public static DependencyProperty RealTimeProperty = DependencyProperty.Register("RealTime", typeof(bool), typeof(SearchBar), new PropertyMetadata(true, RealTimePropertyChanged));
        public static DependencyProperty PlaceholderTextProperty = DependencyProperty.Register("PlaceholderText", typeof(string), typeof(SearchBar), new PropertyMetadata("Search this view", PlaceholderTextChanged));

        public string PlaceholderText
        {
            get => tbSearchText.PlaceholderText;
            set => tbSearchText.PlaceholderText = value;
        }

        private static void PlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as SearchBar;
            obj.PlaceholderText = (string)e.NewValue;
        }

        private static void RealTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as SearchBar;
            obj.RealTime = (bool)e.NewValue;
        }

        private static void SearchViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as SearchBar;
            obj.SearchView = (AdvancedCollectionView)e.NewValue;
            obj.SearchInternal();
        }

        private void SearchInternal()
        {
            lock (lockObject)
            {
                if (SearchView == null) return;
                if (Filter == null || string.IsNullOrWhiteSpace(tbSearchText.Text))
                {
                    SearchView.Filter = null;
                }
                else
                {
                    var text = tbSearchText.Text;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        SearchView.Filter = (x) => { return Filter(x).IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0; };
                        //SearchView.Source = ItemsSource.Cast<object>().Where(x => Filter(x));
                    }
                }
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(sender.Text))
            {
                SearchInternal();
            }
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && RealTime)
            {
                //Set the ItemsSource to be your filtered dataset
                //sender.ItemsSource = dataset;

                var suitableItems = new List<string>();
                suitableItems.AddRange(SearchView.Where(x => Filter(x).IndexOf(sender.Text, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.ToString()));
                if (suitableItems.Count == 0)
                {
                    suitableItems.Add("No results found");
                }
                sender.ItemsSource = suitableItems;
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            SearchInternal();
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = args.SelectedItem.ToString();
        }

        public void ResetQuery()
        {
            tbSearchText.Text = string.Empty;
        }
    }
}
