using System;
using System.Data;
using RealAndVirtualMachine.Machines;
using RealAndVirtualMachine.Memory.Pages;
using RealAndVirtualMachine.Properties;

namespace RealAndVirtualMachine.Memory
{
    public class PageTable
    {
        private readonly RealMachine _realMachine;
        private Random random = new Random();

        private VirtualPage[] m_virtualPages = new VirtualPage[Settings.Default.VirtualPagesCount];

        //Virtual pages getter and setter
        public VirtualPage[] VirtualPages
        {
            get { return m_virtualPages; }
            private set { m_virtualPages = value; }
        }

        //PageTable object constructor
        public PageTable(RealMachine realMachine)
        {
            _realMachine = realMachine;
            for (int i = 0; i < Settings.Default.VirtualPagesCount; i++)
            {
                VirtualPages[i] = new VirtualPage(i);
            }
        }
        //PageTable object constructor
        public PageTable(PageTable oldPageTable) : this(oldPageTable._realMachine)
        {
            for (int i = 0; i < Settings.Default.VirtualPagesCount; i++)
            {
                var oldPage = oldPageTable.VirtualPages[i];
                if (oldPage.IsAllocated)
                {
                    for (int j = 0; j < oldPage.Memory.Count; j++)
                    {
                        this[i].Memory[j] = oldPage.Memory[j];
                    }
                }
            }
        }

        private RealPage FindFreePage()
        {
            int startPage = random.Next(0, Settings.Default.RealPagesCount);
            int currentPage = startPage;
            do
            {
                if (!_realMachine.IsPageAllocated(currentPage))
                {
                    return _realMachine.MemoryPages[currentPage];
                }
                currentPage++;
                //find pages circular, e.g. do not cross over Settings.Default.RealPagesCount
                currentPage %= Settings.Default.RealPagesCount;
            } while (currentPage != startPage);
            throw new InsufficientMemoryException("Could not find free page");
        }

        public int GetPhysicalAddress(int virtualAddress)
        {
            int virtualBlockNr = virtualAddress / Settings.Default.PageSize;
            int shift = virtualAddress % Settings.Default.PageSize;

            if (this[virtualBlockNr] == null)
            {
                throw new InvalidConstraintException("Real page can not be null");
            }
            var realBlockNr = _realMachine.GetPageIndex(this[virtualBlockNr]);
            return realBlockNr * Settings.Default.PageSize + shift;
        }


        public bool IsAllocatedPage(int virtualPageIndex)
        {
            return VirtualPages[virtualPageIndex].IsAllocated;
        }

        //Method is named 'this' because of to declare indexers
        public RealPage this[int index]
        {
            get
            {
                if (index < 0 || index > Settings.Default.VirtualPagesCount - 1)
                {
                    throw new IndexOutOfRangeException("Index must be in range [0.." + (Settings.Default.VirtualPagesCount - 1) + "], current index: " + index);
                }
                if (!VirtualPages[index].IsAllocated)
                {
                    VirtualPages[index].Allocate(FindFreePage());
                }
                return (RealPage)VirtualPages[index].AllocatedToPage;
            }
        }

        public void DeallocateAllPages()
        {
            foreach (var virtualPage in VirtualPages)
            {
                var realPage = virtualPage.AllocatedToPage;
                if (realPage != null)
                {
                    virtualPage.Deallocate(realPage);
                }

            }
        }
    }
}
