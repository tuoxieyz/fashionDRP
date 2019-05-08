using System;
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
using Telerik.Windows.Controls;
using System.Collections;

namespace View.Extension
{
    [TemplatePartAttribute(Name = "PART_AddButton", Type = typeof(RadButton))]
    [TemplatePartAttribute(Name = "PART_DeleteButton", Type = typeof(RadButton))]
    public class UserAddDelItem : ContentControl
    {
        private static readonly Dictionary<string, RoutedCommand> InternalCommands = new Dictionary<string, RoutedCommand>();

        private static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(UserAddDelItem));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty ButtonPositionProperty =
            DependencyProperty.Register("ButtonPosition",
            typeof(ADButtonPosition),
            typeof(UserAddDelItem),
            new PropertyMetadata(ADButtonPosition.Separate, OnButtonPositionPropertyChanged));

        public ADButtonPosition ButtonPosition
        {
            get
            {
                return (ADButtonPosition)GetValue(ButtonPositionProperty);
            }
            set
            {
                SetValue(ButtonPositionProperty, value);
            }
        }

        public static ICommand Add
        {
            get
            {
                return EnsureCommand("Add");
            }
        }

        public static ICommand Delete
        {
            get
            {
                return EnsureCommand("Delete");
            }
        }

        static UserAddDelItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UserAddDelItem), new FrameworkPropertyMetadata(typeof(UserAddDelItem)));

            RegisterCommand(UserAddDelItem.Add, OnAddCommand, CanAddExecute);
            //RegisterCommand(UserAddDelItem.Delete, OnCommitCellEditCommand, CanCommitCellEditExecute);
        }

        private static void OnAddCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (UserAddDelItem)sender;
            var list = item.ParentOfType<UserAddDelItemsControl>();
            var index = ((IList)list.ItemsSource).IndexOf(item.DataContext);
            //((IList)list.ItemsSource).Insert(index+1,new

            e.Handled = true;
        }

        private static void CanAddExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            var item = sender as UserAddDelItem;
            if (item != null)
            {
                var list = item.ParentOfType<UserAddDelItemsControl>();
                if (list != null)
                {
                    if (list.ItemsSource != null)
                    {
                        //var type = list.ItemsSource.GetType();
                        //if(type == typeof(List<>) || type.IsSubclassOf(typeof(List<>)))
                        //{
                        //    //((List<object>)list.ItemsSource).in
                        //}
                        e.CanExecute = list.ItemsSource is IList;
                    }
                }
            }
            e.Handled = true;
        }

        private static void OnButtonPositionPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var item = sender as UserAddDelItem;
            if (item != null)
            {
                item.PlaceButton();
            }
        }

        private void PlaceButton()
        {
            var btnAdd = this.GetTemplateChild("PART_AddButton");
            var btnDelete = this.GetTemplateChild("PART_DeleteButton");

            if (btnAdd != null && btnDelete != null)
            {
                if (this.ButtonPosition == ADButtonPosition.Left)
                {
                    btnAdd.SetValue(Grid.ColumnProperty, 0);
                    btnDelete.SetValue(Grid.ColumnProperty, 1);
                }
                else if (this.ButtonPosition == ADButtonPosition.Right)
                {
                    btnAdd.SetValue(Grid.ColumnProperty, 3);
                    btnDelete.SetValue(Grid.ColumnProperty, 4);
                }
                else
                {
                    btnAdd.SetValue(Grid.ColumnProperty, 0);
                    btnDelete.SetValue(Grid.ColumnProperty, 4);
                }
            }
        }

        private static RoutedCommand EnsureCommand(string commandName)
        {
            if (InternalCommands[commandName] == null)
            {
                var newCommand = new RoutedCommand(commandName, typeof(UserAddDelItem));
                InternalCommands[commandName] = newCommand;
            }

            return InternalCommands[commandName];
        }

        private static void RegisterCommand(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
        {
            CommandManager.RegisterClassCommandBinding(typeof(UserAddDelItem), new CommandBinding(command, executed, canExecute));
        }
    }

    public enum ADButtonPosition
    {
        Left,
        Right,
        /// <summary>
        /// 各在一边
        /// </summary>
        Separate
    }
}
