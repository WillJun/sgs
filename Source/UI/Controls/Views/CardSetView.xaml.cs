﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sanguosha.UI.Controls
{
	/// <summary>
	/// Interaction logic for CardSetView.xaml
	/// </summary>
	public partial class CardSetView : UserControl
	{
		public CardSetView()
		{
			this.InitializeComponent();
		}

        public bool IsGetCardButtonShown
        {
            get { return (bool)GetValue(IsGetCardButtonShownProperty); }
            set { SetValue(IsGetCardButtonShownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsGetCardButtonShown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsGetCardButtonShownProperty =
            DependencyProperty.Register("IsGetCardButtonShown", typeof(bool), typeof(CardSetView), new UIPropertyMetadata(true, new PropertyChangedCallback(_IsGetCardButtonShown_Changed)));

        public static void _IsGetCardButtonShown_Changed(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            CardSetView view = o as CardSetView;
            if (view != null)
            {
                view.btnGetCard.Visibility = (bool)args.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void gridDataSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = gridCardSet.SelectedItem;
            if (item == null) btnGetCard.IsEnabled = false;
            else btnGetCard.IsEnabled = true;
        }
	}
}