using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using RealAndVirtualMachine.Memory;
using RealAndVirtualMachine.Memory.Pages;
using RealAndVirtualMachine.Properties;

namespace RealAndVirtualMachine.Machines
{
    /// <summary>
    /// RealMachine class is imitation of operating systems real machine.
    ///     This class responsible for 
    ///         physical memory management
    ///         file I/O
    ///         indicating which pages are allocated and which are not
    ///         handling multiple virtual machines' processes
    /// </summary>
    public class RealMachine
    {
        private int m_realMemorySize = Settings.Default.PageSize * Settings.Default.RealPagesCount;
        private RealPage[] m_memoryPages = new RealPage[Settings.Default.RealPagesCount];
        private Dictionary<RealPage, int> pagesIndexes = new Dictionary<RealPage, int>();
        public ObservableCollection<VirtualMachine> VirtualMachines { get; set; }

        private int m_ti = Settings.Default.TimerStartValue;

        //Timer register getter and setter
        public int TI
        {
            get { return m_ti; }
            set
            {
                if (value == m_ti) return;
                m_ti = value;
            }
        }

        //Get m_memoryPages values
        public RealPage[] MemoryPages
        {
            get { return m_memoryPages; }
        }

        //RealMachine object constructor with parameter
        public RealMachine(ObservableCollection<VirtualMachine> virtualMachines)
            : this()
        {
            VirtualMachines = virtualMachines;
        }

        //RealMachine cobject constructor without parameter
        public RealMachine()
        {
            VirtualMachines = new ObservableCollection<VirtualMachine>();
            for (int i = 0; i < m_memoryPages.Length; i++)
            {
                var realPage = new RealPage(i);
                m_memoryPages[i] = realPage;
                pagesIndexes.Add(realPage, i);
            }
        }

        //Get page index value
        public int GetPageIndex(RealPage realPage)
        {
            int index;
            if (!pagesIndexes.TryGetValue(realPage, out index))
            {
                throw new KeyNotFoundException("Could not found specified page");
            }
            return index;
        }

        /// <summary>
        /// Reads <see cref="Memory.Word"/> from memory at given physical address
        /// </summary>
        /// <param name="addr">physical memory address</param>
        /// <returns>word that was writen in given address</returns>
        /// <exception cref="IndexOutOfRangeException"> Throws exception if address is out of memory bounds</exception>
        public Word ReadMem(int addr)
        {
            if (addr < 0 || addr > m_realMemorySize - 1)
            {
                throw new IndexOutOfRangeException(
                    "Cannot access memory that is out of bounds, memory range: [0.." + (m_realMemorySize - 1) + "] " +
                    "tried to access: " + addr);
            }
            var pageNr = addr / Settings.Default.PageSize;
            var pageShift = addr % Settings.Default.PageSize;
            return MemoryPages[pageNr][pageShift];
        }

        /// <summary>
        /// Writes <see cref="Memory.Word"/> into memory at given physical address
        /// </summary>
        /// <param name="addr">physical memory address</param>
        /// <param name="data"><see cref="Memory.Word"/> that needs to be written into given address</param>
        /// <exception cref="IndexOutOfRangeException"> Throws exception if address is out of memory bounds</exception>
        public void WriteMem(int addr, Word data)
        {
            if (addr < 0 || addr > m_realMemorySize - 1)
            {
                throw new IndexOutOfRangeException(
                    "Cannot access memory that is out of bounds, memory range: [0.." + (m_realMemorySize - 1) + "] " +
                    "tried to access: " + addr);
            }
            var pageNr = addr / Settings.Default.PageSize;
            var pageShift = addr % Settings.Default.PageSize;
            MemoryPages[pageNr][pageShift] = data;

        }

        internal void AllocatePage(int pageNr, Page allocateToPage)
        {
            if (pageNr < 0 || pageNr > Settings.Default.RealPagesCount - 1)
            {
                throw new IndexOutOfRangeException("page number (" + pageNr + ") is out of range, page number must be between [0.." + (Settings.Default.RealPagesCount - 1) + "]");
            }
            MemoryPages[pageNr].Allocate(allocateToPage);
        }

        internal void DeallocatePage(int pageNr, Page deallocateFromPage)
        {
            if (pageNr < 0 || pageNr > Settings.Default.RealPagesCount - 1)
            {
                throw new IndexOutOfRangeException("page number (" + pageNr + ") is out of range, page number must be between [0.." + (Settings.Default.RealPagesCount - 1) + "]");
            }
            MemoryPages[pageNr].Deallocate(deallocateFromPage);
        }

        internal bool IsPageAllocated(int pageNr)
        {
            return MemoryPages[pageNr].IsAllocated;
        }

        //Fork means to duplicate proccess
        public VirtualMachine ForkVirtualMachine(VirtualMachine virtualMachine)
        {
            VirtualMachine vm;

            try
            {
                vm = new VirtualMachine(virtualMachine);
                VirtualMachines.Add(vm);
                return vm;
            }
            catch (Exception)
            {
                if (virtualMachine != null)
                {
                    virtualMachine.ReleaseResources();
                }
                throw new InsufficientMemoryException("No more memmory");
            }
        }

        //Executes next action of command in file
        public void ExecuteAction(VirtualMachine virtualMachine)
        {
            try
            {
                virtualMachine.DoNextInstruction();
            }
            catch (Exception exception)
            {
                virtualMachine.ReleaseResources();
                VirtualMachines.Remove(virtualMachine);
                MessageBox.Show("Ooops... your program have crased\n" + exception.Message, "Program " + virtualMachine.Name + "(" + virtualMachine.PID + ") have occured error");
            }
        }

        //Until program is finished do tasks
        public void FullyRunAllPrograms()
        {
            while (VirtualMachines.Any(x => !x.IsFinished))
            {
                RunVirtualMachinesUntilTimerInterupt();
            }
        }

        public void RunVirtualMachinesUntilTimerInterupt()
        {
            for (int i = 0; i < VirtualMachines.Count; i++)
            {
                try
                {
                    if (VirtualMachines[i].IsFinished)
                    {
                        continue;
                    }
                    TI = Settings.Default.TimerStartValue;
                    for (; TI > 0; TI--)
                    {
                        if (!VirtualMachines[i].IsFinished)
                        {
                            ExecuteAction(VirtualMachines[i]);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (i < VirtualMachines.Count)
                    {
                        VirtualMachines[i].ReleaseResources();
                        VirtualMachines.Remove(VirtualMachines[i]);
                        MessageBox.Show("Ooops... your program have crased\n" + exception.Message, "Program " + VirtualMachines[i].Name + "(" + VirtualMachines[i].PID + ") have occured error");
                    }
                }
            }
        }
    }
}
