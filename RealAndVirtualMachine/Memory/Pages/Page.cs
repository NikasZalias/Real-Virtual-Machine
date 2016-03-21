using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RealAndVirtualMachine.Annotations;
using RealAndVirtualMachine.Properties;

namespace RealAndVirtualMachine.Memory.Pages
{
    //INotifyPropertyChanged interface notifies clients that a property value has changed.
    abstract public class Page: INotifyPropertyChanged
    {
        private Page m_allocatedToPage;
        public int PageNr { get; protected set; }

        //m_allocatedToPage getter and setter
        public Page AllocatedToPage
        {
            get { return m_allocatedToPage; }
            protected set
            {
                if (m_allocatedToPage == value) return;
                m_allocatedToPage = value;
                OnPropertyChanged("IsAllocated");
                OnPropertyChanged("AllocatedToPage");
            }
        }

        //Constructor
        protected Page(int pageNr)
        {
            PageNr = pageNr;
        }

        //ObservableCollection represents a dynamic data collection that provides notifications when
        //items get added, removed, or when the whole list is refreshed.
        private ObservableCollection<Word> m_memory;

        //m_memory getter and setter
        public ObservableCollection<Word> Memory
        {
            get { return m_memory; }
            protected set
            {
                if (value == m_memory) return;
                m_memory = value;
                OnPropertyChanged("Memory");
            }
        }
        //This atribute indicates the name by which an indexer is known
        //in programming languages that do not support indexers directly.
        [IndexerName("Item")]
        //Method is named 'this' because of to declare indexers
        public Word this[int i]
        {
            get
            {
                if (!IsMemoryAccesable())
                {
                    throw new AllocationException("Can not acces memory that is not allocated");
                }
                if (i < 0 || i > Settings.Default.PageSize)
                {
                    throw new IndexOutOfRangeException("Index must be between [0.." + Settings.Default.PageSize + "], current index: " + i);
                }
                return Memory[i];              
            }
            set
            {
                if (!IsMemoryAccesable())
                {
                    throw new AllocationException("Can not set memory value that is unallocated");
                }
                if (i < 0 || i > Settings.Default.PageSize)
                {
                    throw new IndexOutOfRangeException("Index must be between [0.." + Settings.Default.PageSize + "], current index: " + i);
                }
                if (value == null)
                {
                    throw new NullReferenceException("Word can not be null");
                }
                Memory[i] = value;
                OnPropertyChanged("Memory");  
            }
        }

        //Check if allocated
        public bool IsAllocated
        {
            get
            {
                if (m_allocatedToPage != null)
                {
                    return true;
                }
                return false;
            }
        }      

        protected abstract bool IsMemoryAccesable();
        public abstract void Allocate(Page allocateFor);
        public abstract void Deallocate(Page deallocateFrom);

        public event PropertyChangedEventHandler PropertyChanged;

        //The OnPropertyChanged method is used to raise the event
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));            
        }
    }
}
