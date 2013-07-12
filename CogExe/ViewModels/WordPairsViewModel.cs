﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class WordPairsViewModel : ViewModelBase
	{
		private readonly BindableList<WordPairViewModel> _wordPairs;
		private ListCollectionView _wordPairsView;
		private readonly BindableList<WordPairViewModel> _selectedWordPairs;
		private readonly BindableList<WordPairViewModel> _selectedCorrespondenceWordPairs;

		public WordPairsViewModel(CogProject project, IEnumerable<WordPair> wordPairs, bool areVarietiesInOrder)
			: this(wordPairs.Select(pair => new WordPairViewModel(project, pair, areVarietiesInOrder)))
		{
		}

		public WordPairsViewModel()
			: this(Enumerable.Empty<WordPairViewModel>())
		{
		}

		private WordPairsViewModel(IEnumerable<WordPairViewModel> wordPairs)
		{
			_wordPairs = new BindableList<WordPairViewModel>(wordPairs);
			_wordPairs.CollectionChanged += _wordPairs_CollectionChanged;
			_selectedWordPairs = new BindableList<WordPairViewModel>();
			_selectedCorrespondenceWordPairs = new BindableList<WordPairViewModel>();
		}

		private void _wordPairs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_selectedWordPairs.Clear();
			_selectedCorrespondenceWordPairs.Clear();
		}

		public ObservableList<WordPairViewModel> WordPairs
		{
			get { return _wordPairs; }
		}

		public ICollectionView WordPairsView
		{
			get
			{
				if (_wordPairsView == null)
					_wordPairsView = new ListCollectionView(_wordPairs);
				return _wordPairsView;
			}
		}

		public ObservableList<WordPairViewModel> SelectedWordPairs
		{
			get { return _selectedWordPairs; }
		}

		public ObservableList<WordPairViewModel> SelectedCorrespondenceWordPairs
		{
			get { return _selectedCorrespondenceWordPairs; }
		}

		public string SelectedWordPairsText
		{
			get
			{
				int count = 0;
				var sb = new StringBuilder();
				foreach (WordPairViewModel pair in _wordPairsView)
				{
					if (!_selectedWordPairs.Contains(pair))
						continue;

					if (count > 0)
						sb.AppendLine();

					sb.Append(pair.Sense.Gloss);
					if (!string.IsNullOrEmpty(pair.Sense.Category))
						sb.AppendFormat(" ({0})", pair.Sense.Category);
					sb.AppendLine();

					sb.Append(pair.ModelAlignment.ToString(pair.ModelWordPair.AlignmentNotes));

					sb.AppendFormat("Similarity: {0:p}", pair.PhoneticSimilarityScore);
					sb.AppendLine();
					count++;
					if (count == _selectedWordPairs.Count)
						break;
				}
				return sb.ToString();
			}
		}
	}
}
