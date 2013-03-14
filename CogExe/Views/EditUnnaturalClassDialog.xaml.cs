﻿using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for EditUnnaturalClassDialog.xaml
	/// </summary>
	public partial class EditUnnaturalClassDialog
	{
		public EditUnnaturalClassDialog()
		{
			InitializeComponent();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (ViewUtilities.IsValid(this))
				DialogResult = true;
		}
	}
}