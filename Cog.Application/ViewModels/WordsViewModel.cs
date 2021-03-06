using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class WordsViewModel : ViewModelBase
	{
		public delegate WordsViewModel Factory(ReadOnlyBindableList<WordViewModel> words);

		private readonly IBusyService _busyService;
		private readonly ReadOnlyBindableList<WordViewModel> _words; 
		private ICollectionView _wordsView;
		private readonly BindableList<WordViewModel> _selectedWords;
		private readonly BindableList<WordViewModel> _selectedSegmentWords;
		private SortDescription? _deferredSortDesc;
		private WordViewModel _startWord;
		private readonly SimpleMonitor _selectedWordsMonitor;
		private int _validWordCount;
		private int _invalidWordCount;

		public WordsViewModel(IBusyService busyService, ReadOnlyBindableList<WordViewModel> words)
		{
			_busyService = busyService;
			_words = words;
			_words.CollectionChanged += WordsChanged;
			_selectedWords = new BindableList<WordViewModel>();
			_selectedWords.CollectionChanged += _selectedWords_CollectionChanged;
			_selectedSegmentWords = new BindableList<WordViewModel>();
			_selectedWordsMonitor = new SimpleMonitor();
			AddWords(_words);
			UpdateWordCounts();
		}

		private void WordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddWords(e.NewItems.Cast<WordViewModel>());
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveWords(e.OldItems.Cast<WordViewModel>());
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveWords(e.OldItems.Cast<WordViewModel>());
					AddWords(e.NewItems.Cast<WordViewModel>());
					break;
				case NotifyCollectionChangedAction.Reset:
					_selectedWords.Clear();
					using (_selectedSegmentWords.BulkUpdate())
					{
						_selectedSegmentWords.Clear();
						AddWords(_words);
					}
					break;
			}
			ResetSearch();
			UpdateWordCounts();
		}

		private void AddWords(IEnumerable<WordViewModel> words)
		{
			foreach (WordViewModel word in words)
			{
				if (word.Segments.Any(s => s.IsSelected))
					_selectedSegmentWords.Add(word);
				word.PropertyChanged += word_PropertyChanged;
			}
		}

		private void RemoveWords(IEnumerable<WordViewModel> words)
		{
			foreach (WordViewModel word in words)
			{
				_selectedSegmentWords.Remove(word);
				_selectedWords.Remove(word);
				word.PropertyChanged -= word_PropertyChanged;
			}
		}

		private void word_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsValid")
				UpdateWordCounts();
		}

		private void UpdateWordCounts()
		{
			int validCount = 0, invalidCount = 0;
			foreach (WordViewModel word in _words)
			{
				if (word.IsValid)
					validCount++;
				else
					invalidCount++;
			}
			ValidWordCount = validCount;
			InvalidWordCount = invalidCount;
		}

		internal void UpdateSort(string propertyName, ListSortDirection sortDirection)
		{
			var sortDesc = new SortDescription(propertyName, sortDirection);
			if (_wordsView == null)
				_deferredSortDesc = sortDesc;
			else
				UpdateSort(sortDesc);
		}

		private void UpdateSort(SortDescription sortDesc)
		{
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			if (_wordsView.SortDescriptions.Count == 0)
				_wordsView.SortDescriptions.Add(sortDesc);
			else
				_wordsView.SortDescriptions[0] = sortDesc;
		}

		internal bool FindNext(FindField field, string str)
		{
			if (_words.Count == 0)
			{
				ResetSearch();
				return false;
			}
			if (_selectedWords.Count == 0)
			{
				_startWord = _wordsView.Cast<WordViewModel>().Last();
			}
			else if (_startWord == null)
			{
				_startWord = _selectedWords[0];
			}
			else if (_selectedWords.Contains(_startWord))
			{
				ResetSearch();
				return false;
			}

			List<WordViewModel> words = _wordsView.Cast<WordViewModel>().ToList();
			WordViewModel curWord = _selectedWords.Count == 0 ? _startWord : _selectedWords[0];
			int wordIndex = words.IndexOf(curWord);
			do
			{
				wordIndex = (wordIndex + 1) % words.Count;
				curWord = words[wordIndex];
				bool match = false;
				switch (field)
				{
					case FindField.Form:
						match = curWord.StrRep.Contains(str);
						break;

					case FindField.Gloss:
						match = curWord.Meaning.Gloss.Contains(str);
						break;
				}
				if (match)
				{
					using (_selectedWordsMonitor.Enter())
					{
						_selectedWords.Clear();
						_selectedWords.Add(curWord);
					}
					return true;
				}
			}
			while (_startWord != curWord);
			ResetSearch();
			return false;
		}

		internal void ResetSearch()
		{
			_startWord = null;
		}

		private void _selectedWords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_selectedWordsMonitor.Busy)
				ResetSearch();
		}

		public string SelectedWordsText
		{
			get
			{
				var sb = new StringBuilder();
				int count = 0;
				foreach (WordViewModel word in _wordsView)
				{
					if (!_selectedWords.Contains(word))
						continue;

					if (count > 0)
						sb.AppendLine();

					sb.Append(word.Meaning.Gloss);
					if (!string.IsNullOrEmpty(word.Meaning.Category))
						sb.AppendFormat(" ({0})", word.Meaning.Category);
					sb.AppendLine();

					sb.Append(word.DomainWord);
					sb.AppendLine();
					count++;
					if (count == _selectedWords.Count)
						break;
				}
				return sb.ToString();
			}
		}

		public ReadOnlyObservableList<WordViewModel> Words
		{
			get { return _words; }
		}

		public int ValidWordCount
		{
			get { return _validWordCount; }
			private set { Set(() => ValidWordCount, ref _validWordCount, value); }
		}

		public int InvalidWordCount
		{
			get { return _invalidWordCount; }
			private set { Set(() => InvalidWordCount, ref _invalidWordCount, value); }
		}

		public ICollectionView WordsView

		{
			get { return _wordsView; }
			set
			{
				if (Set(() => WordsView, ref _wordsView, value) && _deferredSortDesc != null)
				{
					UpdateSort(_deferredSortDesc.Value);
					_deferredSortDesc = null;
				}
			}
		}

		public ObservableList<WordViewModel> SelectedWords
		{
			get { return _selectedWords; }
		}

		public ObservableList<WordViewModel> SelectedSegmentWords
		{
			get { return _selectedSegmentWords; }
		}
	}
}
