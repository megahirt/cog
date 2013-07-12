﻿using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class EditAffixViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly CogProject _project;
		private string _strRep;
		private AffixViewModelType _type;
		private string _category;
		private readonly string _title;

		public EditAffixViewModel(CogProject project, Affix affix)
		{
			_title = "Edit Affix";
			_project = project;
			_strRep = affix.StrRep;
			switch (affix.Type)
			{
				case AffixType.Prefix:
					_type = AffixViewModelType.Prefix;
					break;
				case AffixType.Suffix:
					_type = AffixViewModelType.Suffix;
					break;
			}
			_category = affix.Category;
		}

		public EditAffixViewModel(CogProject project)
		{
			_title = "New Affix";
			_project = project;
		}

		public string Title
		{
			get { return _title; }
		}

		public string StrRep
		{
			get { return _strRep; }
			set { Set(() => StrRep, ref _strRep, value); }
		}

		public AffixViewModelType Type
		{
			get { return _type; }
			set { Set(() => Type, ref _type, value); }
		}

		public string Category
		{
			get { return _category; }
			set { Set(() => Category, ref _category, value); }
		}

		string IDataErrorInfo.this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "StrRep":
						if (string.IsNullOrEmpty(_strRep))
							return "Please specify an affix";
						if (!_project.Segmenter.CanSegment(_strRep))
							return "The affix contains invalid segments";
						break;
				}

				return null;
			}
		}

		string IDataErrorInfo.Error
		{
			get { return null; }
		}
	}
}
