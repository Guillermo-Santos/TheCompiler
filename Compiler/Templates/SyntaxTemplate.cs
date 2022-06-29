using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Compiler.Templates
{
    public class SyntaxTemplate : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public enum SyntaxItemType { Basic, Child };
        public string Name { get; set; }
        public string Symbol { get; set; }
        public SyntaxItemType Type { get; set; }
        private ObservableCollection<SyntaxTemplate> m_children;
        public ObservableCollection<SyntaxTemplate> Children
        {
            get
            {
                if (m_children == null)
                {
                    m_children = new ObservableCollection<SyntaxTemplate>();
                }
                return m_children;
            }
            set
            {
                m_children = value;
            }
        }

        private bool m_isExpanded;
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                if (m_isExpanded != value)
                {
                    m_isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
