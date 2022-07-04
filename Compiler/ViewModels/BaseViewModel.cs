using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Compiler.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {

        bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        public void OnAppearing()
        {
            IsBusy = true;
        }
        /// <summary>
        /// Load an <see cref="IEnumerable{T}"/> to a <seealso cref="ObservableCollection{T}"/>
        /// </summary>
        /// <typeparam name="T">General type of the <see cref="IEnumerable{T}"/> and <seealso cref="ObservableCollection{T}"/></typeparam>
        /// <param name="items"><see cref="IEnumerable{T}"/> to be passed</param>
        /// <param name="list"><seealso cref="ObservableCollection{T}"/> that recieve the data</param>
        protected void LoadCollectionsData<T>(IEnumerable<T> items, ref ObservableCollection<T> list)
        {
            list.Clear();
            foreach (T item in items)
            {
                list.Add(item);
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
