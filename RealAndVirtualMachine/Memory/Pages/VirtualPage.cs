using System;
using System.Text;

namespace RealAndVirtualMachine.Memory.Pages
{
    public class VirtualPage : Page
    {
        //Constructor to create VirtualPage object
        //base constructor will be called first
        public VirtualPage(int pageNr) : base(pageNr)
        {
        }

        protected override bool IsMemoryAccesable()
        {
            return IsAllocated;
        }

        //Inherited method from Page.cs to allocate page
        public override void Allocate(Page allocateFor)
        {
            if (AllocatedToPage == allocateFor)
            {
                return;
            }
            if (!(allocateFor is RealPage))
            {
                throw new AllocationException("It is possible to allocate real pages to virtual pages only");
            }
            if (IsAllocated)
            {
                throw new AllocationException("Cannot allocate page that is already allocated");
            }
            if (allocateFor == null)
            {
                throw new NullReferenceException("Can not allocate to Null reference page");
            }
            AllocatedToPage = allocateFor;
            if (allocateFor.AllocatedToPage != this)
            {
                allocateFor.Allocate(this);
                this.Memory = allocateFor.Memory;
            }
        }

        //Inherited method from Page.cs to deallocate page
        public override void Deallocate(Page deallocateFrom)
        {
            if (deallocateFrom == null)
            {
                throw new NullReferenceException("Can not deallocate page from Null reference");
            }
            if (!(deallocateFrom is RealPage))
            {
                throw new AllocationException("It is possible to allocate (and deallocate) real pages to virtual pages only");
            }
            if (AllocatedToPage != deallocateFrom)
            {
                throw new AllocationException("Can not deallocate from page, to whom this page is not allocated");
            }
            if (deallocateFrom.AllocatedToPage == this)
            {
                AllocatedToPage = null;
                deallocateFrom.Deallocate(this);
            }
            this.Memory = null;
        }

        public override string ToString()
        {
            var builder = new StringBuilder("Virtual Page [Nr: " + PageNr + ", IsAllocated:" + IsAllocated);
            if (IsAllocated)
            {
                builder.Append(", AllocatedTo:" + AllocatedToPage.PageNr);
            }
            builder.Append("]");
            return builder.ToString();
        }
    }
}
