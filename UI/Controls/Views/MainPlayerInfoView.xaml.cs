﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for MainPlayerInfoView.xaml
    /// </summary>
    public partial class MainPlayerInfoView : PlayerInfoViewBase
    {
        public MainPlayerInfoView()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(PlayerInfoView_DataContextChanged);
            _OnPropertyChanged = new PropertyChangedEventHandler(model_PropertyChanged);
        }

        private PropertyChangedEventHandler _OnPropertyChanged;

        void PlayerInfoView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PlayerInfoViewModel model = e.OldValue as PlayerInfoViewModel;
            if (model != null)
            {
                model.PropertyChanged -= _OnPropertyChanged;
            }
            model = e.NewValue as PlayerInfoViewModel;
            if (model != null)
            {
                model.PropertyChanged += _OnPropertyChanged;
                cbRoleBox.DataContext = model.PossibleRoles;
            }
        }       

        void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PlayerInfoViewModel model = sender as PlayerInfoViewModel;
            if (e.PropertyName == "CurrentPhase")
            {
                if (model.CurrentPhase == Core.Games.TurnPhase.Inactive)
                {
                    imgPhase.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    imgPhase.Visibility = System.Windows.Visibility.Visible;
                    Storyboard animation = Resources["sbPhaseChange"] as Storyboard;
                    animation.Begin(this);
                }
            }
            else if (e.PropertyName == "PossibleRoles")
            {
                cbRoleBox.DataContext = model.PossibleRoles;
            }
        }

        protected override CardStack HandCardStackTemplate
        {
            get { return handCardPlaceHolder; }
        }
    }
}